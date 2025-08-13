using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

[Serializable] public class PlayerInfo { public string name; public long joinedAt; public bool isHost; }
[Serializable]
public class RoomInfo
{
    public string hostUid;
    public string hostToken;  // hoặc bỏ nếu dùng isHost
    public string status;     // "lobby" | "loading" | "started"
    public SceneEvent sceneToLoad;
    public Dictionary<string, PlayerInfo> players;

    [Serializable] public class SceneEvent { public int index; public bool trigger; }
}

public class RoomService : MonoBehaviour
{
    public static RoomService I;
    DatabaseReference db;
    public string CurrentRoomId { get; private set; }
    public event Action<string, Dictionary<string, PlayerInfo>> OnPlayersChanged;
    public event Action<int> OnSceneLoadTriggered;
    public event Action<List<(string roomId, RoomInfo info)>> OnRoomListChanged;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    void Start() => db = FirebaseDatabase.DefaultInstance.RootReference;

    // ======= ROOM LIST =======
    DatabaseReference roomsRef;
    void ListenRooms()
    {
        roomsRef = FirebaseDatabase.DefaultInstance.GetReference("rooms");
        roomsRef.ValueChanged += (s, e) =>
        {
            var list = new List<(string, RoomInfo)>();
            if (e.Snapshot != null && e.Snapshot.Exists)
            {
                foreach (var r in e.Snapshot.Children)
                {
                    var json = r.GetRawJsonValue();
                    var info = JsonUtility.FromJson<RoomInfo>(json);
                    list.Add((r.Key, info));
                }
            }
            OnRoomListChanged?.Invoke(list);
        };
    }
    public void StartRoomDirectory() => ListenRooms();

    // ======= CREATE ROOM =======
    public async Task<string> CreateRoom(int gameplayBuildIndex)
    {
        string uid = AuthManager.I.CurrentUserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập.");

        string roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var roomRef = db.Child("rooms").Child(roomId);

        var tokenTuple = SecureTokenStore.TryLoad();
        var hostToken = tokenTuple.ok ? tokenTuple.idToken : "N/A";

        RoomInfo room = new RoomInfo
        {
            hostUid = uid,
            hostToken = hostToken,
            status = "lobby",
            sceneToLoad = new RoomInfo.SceneEvent { index = gameplayBuildIndex, trigger = false },
            players = new Dictionary<string, PlayerInfo>()
        };
        room.players[uid] = new PlayerInfo
        {
            name = AuthManager.I.DisplayName,
            joinedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            isHost = true
        };

        await roomRef.SetRawJsonValueAsync(JsonUtility.ToJson(room));
        CurrentRoomId = roomId;

        // Lưu roomId vào PlayerPrefs hoặc file
        PlayerPrefs.SetString("lastRoomId", roomId); PlayerPrefs.Save();
        return roomId;
    }

    // ======= JOIN ROOM =======
    public async Task JoinRoom(string roomId)
    {
        string uid = AuthManager.I.CurrentUserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập.");

        var player = new PlayerInfo
        {
            name = AuthManager.I.DisplayName,
            joinedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            isHost = false
        };

        await db.Child("rooms").Child(roomId).Child("players").Child(uid)
            .SetRawJsonValueAsync(JsonUtility.ToJson(player));

        CurrentRoomId = roomId;
        PlayerPrefs.SetString("lastRoomId", roomId); PlayerPrefs.Save();

        SubscribeRoom(roomId);
    }

    // ======= SUBSCRIBE ROOM CHANGES =======
    DatabaseReference roomRef, playersRef, sceneRef;
    void SubscribeRoom(string roomId)
    {
        UnsubscribeRoom();

        roomRef = FirebaseDatabase.DefaultInstance.GetReference("rooms").Child(roomId);

        // Players list
        playersRef = roomRef.Child("players");
        playersRef.ValueChanged += (s, e) =>
        {
            var dict = new Dictionary<string, PlayerInfo>();
            if (e.Snapshot != null && e.Snapshot.Exists)
            {
                foreach (var ch in e.Snapshot.Children)
                {
                    var info = JsonUtility.FromJson<PlayerInfo>(ch.GetRawJsonValue());
                    dict[ch.Key] = info;
                }
            }
            OnPlayersChanged?.Invoke(roomId, dict);
        };

        // Scene event
        sceneRef = roomRef.Child("sceneToLoad");
        sceneRef.ValueChanged += (s, e) =>
        {
            if (e.Snapshot == null || !e.Snapshot.Exists) return;
            int idx = Convert.ToInt32(e.Snapshot.Child("index").Value ?? -1);
            bool trigger = Convert.ToBoolean(e.Snapshot.Child("trigger").Value ?? false);
            if (trigger && idx >= 0) OnSceneLoadTriggered?.Invoke(idx);
        };
    }

    void UnsubscribeRoom()
    {
        if (playersRef != null) { playersRef.ValueChanged -= null; playersRef = null; }
        if (sceneRef != null) { sceneRef.ValueChanged -= null; sceneRef = null; }
        roomRef = null;
    }

    // ======= START GAME (HOST TRIGGER) =======
    public async Task HostTriggerStart()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        await db.Child("rooms").Child(CurrentRoomId).Child("sceneToLoad").Child("trigger").SetValueAsync(true);
        await db.Child("rooms").Child(CurrentRoomId).Child("status").SetValueAsync("loading");
    }

    // ======= LEAVE ROOM =======
    public async Task LeaveRoom()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        var uid = AuthManager.I.CurrentUserId;
        var roomPath = db.Child("rooms").Child(CurrentRoomId);

        // Lấy hostUid để biết mình là host?
        string hostUid = null;
        await roomPath.Child("hostUid").GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.Result != null && t.Result.Exists) hostUid = t.Result.Value?.ToString();
        });

        if (uid == hostUid)
        {
            // Host thoát: xóa luôn room
            await roomPath.RemoveValueAsync();
        }
        else
        {
            // Client thoát: chỉ xóa mình trong players
            await roomPath.Child("players").Child(uid).RemoveValueAsync();
        }

        PlayerPrefs.DeleteKey("lastRoomId");
        CurrentRoomId = null;
        UnsubscribeRoom();
    }
}
