using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class RoomService : MonoBehaviour
{
    public static RoomService I;
    DatabaseReference db;

    public string CurrentRoomId { get; private set; }
    public string LastKnownHostUid { get; private set; }   // Cache hostUid cho UI

    public event Action<List<(string roomId, RoomInfo info)>> OnRoomListChanged;
    public event Action<string, Dictionary<string, PlayerInfo>> OnPlayersChanged;
    public event Action<int, string> OnSceneLoadTriggered;

    const string PPKey = "lastRoomId";

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // === Utils ===
    static async Task<T> WithTimeout<T>(Task<T> task, int ms = 10000)
    {
        var finished = await Task.WhenAny(task, Task.Delay(ms));
        if (finished == task) return task.Result;
        throw new TimeoutException("Firebase operation timed out.");
    }

    static async Task WithTimeout(Task task, int ms = 10000)
    {
        var finished = await Task.WhenAny(task, Task.Delay(ms));
        if (finished == task) { await task; return; }
        throw new TimeoutException("Firebase operation timed out.");
    }

    // === Danh sách phòng (lobby listing) ===
    DatabaseReference roomsRef;
    EventHandler<ValueChangedEventArgs> _roomsHandler;

    public void StartRoomDirectory()
    {
        StopRoomDirectory();
        roomsRef = FirebaseDatabase.DefaultInstance.GetReference("rooms");
        _roomsHandler = (s, e) =>
        {
            var list = new List<(string, RoomInfo)>();
            if (e.Snapshot != null && e.Snapshot.Exists)
            {
                foreach (var r in e.Snapshot.Children)
                {
                    var json = r.GetRawJsonValue();
                    var info = !string.IsNullOrEmpty(json) ? JsonUtility.FromJson<RoomInfo>(json) : null;
                    list.Add((r.Key, info));
                }
            }
            OnRoomListChanged?.Invoke(list);
        };
        roomsRef.ValueChanged += _roomsHandler;
    }

    public void StopRoomDirectory()
    {
        if (roomsRef != null && _roomsHandler != null)
        {
            roomsRef.ValueChanged -= _roomsHandler;
            _roomsHandler = null;
            roomsRef = null;
        }
    }

    // === Tạo phòng ===
    public async Task<string> CreateRoom(int gameplayBuildIndex)
    {
        var uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập Firebase.");

        string roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var roomRef = db.Child("rooms").Child(roomId);

        var room = new RoomInfo
        {
            hostUid = uid,
            hostToken = "",
            status = "lobby",
            sceneToLoad = new SceneEvent
            {
                index = gameplayBuildIndex,
                trigger = false,
                roundId = "",
                triggerAt = 0
            },
            players = new Dictionary<string, PlayerInfo>()
        };
        room.players[uid] = new PlayerInfo
        {
            name = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName ?? uid,
            joinedAt = Now(),
            isHost = true
        };

        await WithTimeout(roomRef.SetRawJsonValueAsync(JsonUtility.ToJson(room)), 10000);

        CurrentRoomId = roomId;
        LastKnownHostUid = uid; // cache lại
        SaveRoomId(roomId);
        SubscribeRoom(roomId);

        // ✅ Gọi OnPlayersChanged ngay lập tức cho UI, không cần chờ Firebase event
        OnPlayersChanged?.Invoke(roomId, room.players);

        return roomId;
    }


    // === Join phòng ===
    public async Task JoinRoom(string roomId)
    {
        var uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập Firebase.");

        // Lấy hostUid từ phòng
        var hostSnap = await WithTimeout(db.Child("rooms").Child(roomId).Child("hostUid").GetValueAsync(), 10000);
        string hostUid = hostSnap.Exists ? hostSnap.Value?.ToString() : null;
        LastKnownHostUid = hostUid;

        var player = new PlayerInfo
        {
            name = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName ?? uid,
            joinedAt = Now(),
            isHost = (uid == hostUid) // ✅ nếu là hostUid thì luôn set isHost = true
        };

        await WithTimeout(
            db.Child("rooms").Child(roomId).Child("players").Child(uid)
              .SetRawJsonValueAsync(JsonUtility.ToJson(player)), 10000
        );

        CurrentRoomId = roomId;
        SaveRoomId(roomId);
        SubscribeRoom(roomId);
    }

    // === Lắng nghe phòng hiện tại ===
    DatabaseReference playersRef, sceneRef, roomRefForCurrent;
    EventHandler<ValueChangedEventArgs> _playersHandler, _sceneHandler;

    void SubscribeRoom(string roomId)
    {
        UnsubscribeRoom();
        roomRefForCurrent = FirebaseDatabase.DefaultInstance.GetReference("rooms").Child(roomId);

        playersRef = roomRefForCurrent.Child("players");
        _playersHandler = (s, e) =>
        {
            var dict = new Dictionary<string, PlayerInfo>();
            if (e.Snapshot != null && e.Snapshot.Exists)
            {
                foreach (var ch in e.Snapshot.Children)
                {
                    var json = ch.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(json))
                        dict[ch.Key] = JsonUtility.FromJson<PlayerInfo>(json);
                }
            }
            OnPlayersChanged?.Invoke(roomId, dict);
        };
        playersRef.ValueChanged += _playersHandler;

        sceneRef = roomRefForCurrent.Child("sceneToLoad");
        _sceneHandler = (s, e) =>
        {
            if (e.Snapshot == null || !e.Snapshot.Exists) return;
            var idxObj = e.Snapshot.Child("index").Value;
            var trigObj = e.Snapshot.Child("trigger").Value;
            var ridObj = e.Snapshot.Child("roundId").Value;

            if (idxObj == null || trigObj == null) return;
            int idx = Convert.ToInt32(idxObj);
            bool trig = Convert.ToBoolean(trigObj);
            string rid = ridObj?.ToString() ?? "";
            if (trig && idx >= 0) OnSceneLoadTriggered?.Invoke(idx, rid);
        };
        sceneRef.ValueChanged += _sceneHandler;

        // Lắng nghe hostUid thay đổi
        roomRefForCurrent.Child("hostUid").ValueChanged += (s, e) =>
        {
            if (e.Snapshot != null && e.Snapshot.Exists)
                LastKnownHostUid = e.Snapshot.Value?.ToString();
        };
    }

    void UnsubscribeRoom()
    {
        if (playersRef != null && _playersHandler != null)
        {
            playersRef.ValueChanged -= _playersHandler;
            _playersHandler = null;
            playersRef = null;
        }
        if (sceneRef != null && _sceneHandler != null)
        {
            sceneRef.ValueChanged -= _sceneHandler;
            _sceneHandler = null;
            sceneRef = null;
        }
        roomRefForCurrent = null;
    }

    // === Host ấn "Bắt đầu" → mọi người load scene ===
    public async Task HostTriggerStart()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        var path = db.Child("rooms").Child(CurrentRoomId);

        string newRoundId = Guid.NewGuid().ToString("N");
        long now = Now();

        await WithTimeout(path.Child("status").SetValueAsync("loading"), 10000);
        await WithTimeout(path.Child("sceneToLoad").UpdateChildrenAsync(new Dictionary<string, object>
        {
            ["trigger"] = true,
            ["roundId"] = newRoundId,
            ["triggerAt"] = now
        }), 10000);
    }

    // === Thoát phòng ===
    public async Task LeaveRoom()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        var path = db.Child("rooms").Child(CurrentRoomId);
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        string hostUid = null;
        var snap = await WithTimeout(path.Child("hostUid").GetValueAsync(), 10000);
        if (snap.Exists) hostUid = snap.Value?.ToString();

        if (uid == hostUid)
        {
            // Host thoát: xoá cả phòng
            await WithTimeout(path.RemoveValueAsync(), 10000);
        }
        else
        {
            // Client thoát: chỉ xoá player
            await WithTimeout(path.Child("players").Child(uid).RemoveValueAsync(), 10000);
        }

        DeleteRoomId();
        CurrentRoomId = null;
        LastKnownHostUid = null;
        UnsubscribeRoom();

        // ✅ Bắn event rỗng cho UI clear ngay
        OnPlayersChanged?.Invoke("", new Dictionary<string, PlayerInfo>());
        OnRoomListChanged?.Invoke(new List<(string, RoomInfo)>());
    }


    // === Lưu/Xóa roomId local ===
    static void SaveRoomId(string id) { PlayerPrefs.SetString(PPKey, id); PlayerPrefs.Save(); }
    public static string LoadRoomId() => PlayerPrefs.GetString(PPKey, "");
    static void DeleteRoomId() { PlayerPrefs.DeleteKey(PPKey); }
}
