using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class RoomService : MonoBehaviour
{
    public static RoomService I;
    DatabaseReference db;

    public string CurrentRoomId { get; private set; }
    public event Action<List<(string roomId, RoomInfo info)>> OnRoomListChanged;
    public event Action<string, Dictionary<string, PlayerInfo>> OnPlayersChanged;
    public event Action<int> OnSceneLoadTriggered;

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

    // === Danh sách phòng (lobby listing) ===
    DatabaseReference roomsRef;
    public void StartRoomDirectory()
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

    // === Tạo phòng ===
    public async Task<string> CreateRoom(int gameplayBuildIndex)
    {
        var uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập Firebase.");

        string roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var roomRef = db.Child("rooms").Child(roomId);

        // Lấy token đã lưu (nếu bạn muốn dùng hostToken = uid thì có thể đặt luôn hostToken = uid)
        var (ok, _, _, token) = SecureTokenStore.TryLoad();

        var room = new RoomInfo
        {
            hostUid = uid,
            hostToken = ok ? uid : uid, // theo yêu cầu: có thể dùng userId làm hostToken
            status = "lobby",
            sceneToLoad = new SceneEvent { index = gameplayBuildIndex, trigger = false },
            players = new Dictionary<string, PlayerInfo>()
        };
        room.players[uid] = new PlayerInfo { name = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName ?? uid, joinedAt = Now(), isHost = true };

        await roomRef.SetRawJsonValueAsync(JsonUtility.ToJson(room));
        CurrentRoomId = roomId;
        SaveRoomId(roomId);
        SubscribeRoom(roomId);
        return roomId;
    }

    // === Join phòng ===
    public async Task JoinRoom(string roomId)
    {
        var uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập Firebase.");

        var player = new PlayerInfo
        {
            name = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName ?? uid,
            joinedAt = Now(),
            isHost = false
        };
        await db.Child("rooms").Child(roomId).Child("players").Child(uid)
            .SetRawJsonValueAsync(JsonUtility.ToJson(player));

        CurrentRoomId = roomId;
        SaveRoomId(roomId);
        SubscribeRoom(roomId);
    }

    // === Lắng nghe phòng hiện tại ===
    DatabaseReference playersRef, sceneRef;
    void SubscribeRoom(string roomId)
    {
        UnsubscribeRoom();
        var roomRef = FirebaseDatabase.DefaultInstance.GetReference("rooms").Child(roomId);

        playersRef = roomRef.Child("players");
        playersRef.ValueChanged += (s, e) =>
        {
            var dict = new Dictionary<string, PlayerInfo>();
            if (e.Snapshot != null && e.Snapshot.Exists)
                foreach (var ch in e.Snapshot.Children)
                    dict[ch.Key] = JsonUtility.FromJson<PlayerInfo>(ch.GetRawJsonValue());
            OnPlayersChanged?.Invoke(roomId, dict);
        };

        sceneRef = roomRef.Child("sceneToLoad");
        sceneRef.ValueChanged += (s, e) =>
        {
            if (e.Snapshot == null || !e.Snapshot.Exists) return;
            var idxObj = e.Snapshot.Child("index").Value;
            var trigObj = e.Snapshot.Child("trigger").Value;
            if (idxObj == null || trigObj == null) return;
            int idx = Convert.ToInt32(idxObj);
            bool trig = Convert.ToBoolean(trigObj);
            if (trig && idx >= 0) OnSceneLoadTriggered?.Invoke(idx);
        };
    }

    void UnsubscribeRoom()
    {
        if (playersRef != null) { playersRef.ValueChanged -= null; playersRef = null; }
        if (sceneRef != null) { sceneRef.ValueChanged -= null; sceneRef = null; }
    }

    // === Host ấn "Bắt đầu" → mọi người load scene ===
    public async Task HostTriggerStart()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        var path = db.Child("rooms").Child(CurrentRoomId);
        await path.Child("status").SetValueAsync("loading");
        await path.Child("sceneToLoad").Child("trigger").SetValueAsync(true);
    }

    // === Thoát phòng ===
    public async Task LeaveRoom()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;
        var path = db.Child("rooms").Child(CurrentRoomId);
        string uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        string hostUid = null;
        var snap = await path.Child("hostUid").GetValueAsync();
        if (snap.Exists) hostUid = snap.Value?.ToString();

        if (uid == hostUid)
        {
            // Chủ phòng thoát: xóa phòng
            await path.RemoveValueAsync();
        }
        else
        {
            // Client thoát: xóa mình khỏi players
            await path.Child("players").Child(uid).RemoveValueAsync();
        }

        DeleteRoomId();
        CurrentRoomId = null;
        UnsubscribeRoom();
    }

    // === Lưu/Xóa roomId local ===
    static void SaveRoomId(string id) { PlayerPrefs.SetString(PPKey, id); PlayerPrefs.Save(); }
    public static string LoadRoomId() => PlayerPrefs.GetString(PPKey, "");
    static void DeleteRoomId() { PlayerPrefs.DeleteKey(PPKey); }

    static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
