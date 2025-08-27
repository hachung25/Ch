using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

[Serializable]

public class RoomService : MonoBehaviour
{
    public static RoomService I;
    private DatabaseReference _db;

    public string CurrentRoomId { get; private set; }
    public string LastKnownHostUid { get; private set; }

    public event Action<List<(string roomId, RoomInfo info)>> OnRoomListChanged;
    public event Action<string, Dictionary<string, PlayerInfo>> OnPlayersChanged;
    public event Action<string> OnHostChanged;
    public event Action<int, string> OnSceneLoadTriggered;

    const string PPKey = "lastRoomId";

    // ===== cache để PlayerSpawner đọc =====
    private Dictionary<string, PlayerInfo> _cachedPlayers = new();
    public Dictionary<string, PlayerInfo> GetPlayersSnapshot() => _cachedPlayers;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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

    // ===== Directory =====
    DatabaseReference _roomsRef;
    EventHandler<ValueChangedEventArgs> _roomsHandler;

    public void StartRoomDirectory()
    {
        StopRoomDirectory();
        _roomsRef = FirebaseDatabase.DefaultInstance.GetReference("rooms");
        _roomsHandler = (s, e) =>
        {
            var list = new List<(string, RoomInfo)>();
            if (e.Snapshot != null && e.Snapshot.Exists)
            {
                foreach (var r in e.Snapshot.Children)
                {
                    var info = new RoomInfo
                    {
                        hostUid = r.Child("hostUid").Value?.ToString(),
                        hostToken = r.Child("hostToken").Value?.ToString(),
                        status = r.Child("status").Value?.ToString() ?? "lobby",
                        sceneToLoad = new SceneEvent
                        {
                            index = r.Child("sceneToLoad").Child("index").Value != null ? Convert.ToInt32(r.Child("sceneToLoad").Child("index").Value) : -1,
                            trigger = r.Child("sceneToLoad").Child("trigger").Value != null && Convert.ToBoolean(r.Child("sceneToLoad").Child("trigger").Value),
                            roundId = r.Child("sceneToLoad").Child("roundId").Value?.ToString() ?? "",
                            triggerAt = r.Child("sceneToLoad").Child("triggerAt").Value != null ? Convert.ToInt64(r.Child("sceneToLoad").Child("triggerAt").Value) : 0
                        },
                        players = new Dictionary<string, PlayerInfo>()
                    };

                    var pNode = r.Child("players");
                    if (pNode.Exists)
                    {
                        foreach (var p in pNode.Children)
                        {
                            var pj = p.GetRawJsonValue();
                            if (!string.IsNullOrEmpty(pj))
                                info.players[p.Key] = JsonUtility.FromJson<PlayerInfo>(pj);
                        }
                    }

                    list.Add((r.Key, info));
                }
            }
            OnRoomListChanged?.Invoke(list);
        };
        _roomsRef.ValueChanged += _roomsHandler;
    }

    public void StopRoomDirectory()
    {
        if (_roomsRef != null && _roomsHandler != null)
        {
            _roomsRef.ValueChanged -= _roomsHandler;
            _roomsHandler = null;
            _roomsRef = null;
        }
    }

    // ===== Create & Join =====
    public async Task<string> CreateRoomAndJoin()
    {
        var uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập Firebase.");

        string roomId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var roomRef = _db.Child("rooms").Child(roomId);
        long now = NowMs();

        var room = new RoomInfo
        {
            hostUid = uid,
            hostToken = "",
            status = "lobby",
            sceneToLoad = new SceneEvent { index = -1, trigger = false, roundId = "", triggerAt = 0 },
            players = new Dictionary<string, PlayerInfo>()
        };

        string displayName = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName
                             ?? PlayerName.Current
                             ?? uid;

        int charIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        var me = new PlayerInfo
        {
            name = displayName,
            joinedAt = now,
            isHost = true,
            characterIndex = charIndex
        };

        await WithTimeout(roomRef.SetRawJsonValueAsync(JsonUtility.ToJson(room)), 10000);
        await WithTimeout(roomRef.Child("players").Child(uid).SetRawJsonValueAsync(JsonUtility.ToJson(me)), 10000);

        long expiryAt = now + 5 * 60 * 1000;
        await WithTimeout(roomRef.Child("expiryAt").SetValueAsync(expiryAt), 5000);

        try { roomRef.Child("players").Child(uid).OnDisconnect().RemoveValue(); } catch { }

        CurrentRoomId = roomId;
        LastKnownHostUid = uid;
        SaveRoomId(roomId);
        SubscribeRoom(roomId);

        OnPlayersChanged?.Invoke(roomId, new Dictionary<string, PlayerInfo> { { uid, me } });
        OnHostChanged?.Invoke(uid);

        _ = AutoDeleteIfExpired(roomId);
        return roomId;
    }

    public async Task JoinRoom(string roomId)
    {
        var uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) throw new Exception("Chưa đăng nhập Firebase.");

        var roomPath = _db.Child("rooms").Child(roomId);
        var exists = await WithTimeout(roomPath.GetValueAsync(), 10000);
        if (!exists.Exists) throw new Exception("Phòng không tồn tại.");

        var hostSnap = await WithTimeout(roomPath.Child("hostUid").GetValueAsync(), 8000);
        LastKnownHostUid = hostSnap.Exists ? hostSnap.Value?.ToString() : null;

        string displayName = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName
                             ?? PlayerName.Current
                             ?? uid;

        int charIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        var player = new PlayerInfo
        {
            name = displayName,
            joinedAt = NowMs(),
            isHost = (uid == LastKnownHostUid),
            characterIndex = charIndex
        };

        await WithTimeout(roomPath.Child("players").Child(uid).SetRawJsonValueAsync(JsonUtility.ToJson(player)), 10000);
        try { roomPath.Child("players").Child(uid).OnDisconnect().RemoveValue(); } catch { }

        CurrentRoomId = roomId;
        SaveRoomId(roomId);
        SubscribeRoom(roomId);

        _ = AutoDeleteIfExpired(roomId);
    }

    // ===== Subscribe a room =====
    DatabaseReference _playersRef, _sceneRef, _hostUidRef;
    EventHandler<ValueChangedEventArgs> _playersHandler, _sceneHandler, _hostUidHandler;

    void SubscribeRoom(string roomId)
    {
        UnsubscribeRoom();

        var roomRef = FirebaseDatabase.DefaultInstance.GetReference("rooms").Child(roomId);

        _playersRef = roomRef.Child("players");
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
            _cachedPlayers = dict; // ✅ lưu cache để PlayerSpawner đọc
            OnPlayersChanged?.Invoke(roomId, dict);
        };
        _playersRef.ValueChanged += _playersHandler;

        _hostUidRef = roomRef.Child("hostUid");
        _hostUidHandler = (s, e) =>
        {
            var newHost = e.Snapshot?.Value?.ToString();
            if (!string.IsNullOrEmpty(newHost) && newHost != LastKnownHostUid)
            {
                LastKnownHostUid = newHost;
                OnHostChanged?.Invoke(newHost);
            }
        };
        _hostUidRef.ValueChanged += _hostUidHandler;

        _sceneRef = roomRef.Child("sceneToLoad");
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
        _sceneRef.ValueChanged += _sceneHandler;
    }

    void UnsubscribeRoom()
    {
        if (_playersRef != null && _playersHandler != null) { _playersRef.ValueChanged -= _playersHandler; _playersHandler = null; _playersRef = null; }
        if (_sceneRef != null && _sceneHandler != null) { _sceneRef.ValueChanged -= _sceneHandler; _sceneHandler = null; _sceneRef = null; }
        if (_hostUidRef != null && _hostUidHandler != null) { _hostUidRef.ValueChanged -= _hostUidHandler; _hostUidHandler = null; _hostUidRef = null; }
    }

    // ===== Host start → broadcast scene =====
    public async Task HostTriggerStart(int gameplayBuildIndex)
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;

        var path = _db.Child("rooms").Child(CurrentRoomId);
        string myUid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        var hostSnap = await WithTimeout(path.Child("hostUid").GetValueAsync(), 8000);
        var hostUid = hostSnap?.Value?.ToString();
        if (myUid != hostUid) throw new Exception("Chỉ host mới được Start.");

        string newRoundId = Guid.NewGuid().ToString("N");
        long now = NowMs();

        await WithTimeout(path.Child("status").SetValueAsync("started"), 8000);
        await WithTimeout(path.Child("sceneToLoad").UpdateChildrenAsync(new Dictionary<string, object>
        {
            ["index"] = gameplayBuildIndex,
            ["trigger"] = true,
            ["roundId"] = newRoundId,
            ["triggerAt"] = now
        }), 8000);

        await WithTimeout(path.Child("expiryAt").SetValueAsync(now + 60L * 60 * 1000), 5000);
    }

    // ===== Leave / Delete =====
    public async Task LeaveRoom()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;

        var path = _db.Child("rooms").Child(CurrentRoomId);
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        await WithTimeout(path.Child("players").Child(uid).RemoveValueAsync(), 10000);

        var playersSnap = await WithTimeout(path.Child("players").GetValueAsync(), 8000);
        if (!playersSnap.Exists || playersSnap.ChildrenCount == 0)
            await WithTimeout(path.RemoveValueAsync(), 8000);

        ResetLocal();
        UnsubscribeRoom();
        OnPlayersChanged?.Invoke("", new Dictionary<string, PlayerInfo>());
    }

    public async Task DeleteRoom()
    {
        if (string.IsNullOrEmpty(CurrentRoomId)) return;

        var path = _db.Child("rooms").Child(CurrentRoomId);
        string myUid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        var hostSnap = await WithTimeout(path.Child("hostUid").GetValueAsync(), 8000);
        var hostUid = hostSnap?.Value?.ToString();
        if (myUid != hostUid) throw new Exception("Chỉ host mới được xoá phòng.");

        await WithTimeout(path.RemoveValueAsync(), 10000);

        ResetLocal();
        UnsubscribeRoom();
        OnPlayersChanged?.Invoke("", new Dictionary<string, PlayerInfo>());
    }

    // ===== Local =====
    static void SaveRoomId(string id) { PlayerPrefs.SetString(PPKey, id); PlayerPrefs.Save(); }
    public void ResetLocal()
    {
        CurrentRoomId = null;
        LastKnownHostUid = null;
        PlayerPrefs.DeleteKey(PPKey);
        _cachedPlayers.Clear();
    }

    // ===== TTL watchdog =====
    async Task AutoDeleteIfExpired(string roomId)
    {
        try
        {
            while (true)
            {
                await Task.Delay(15000);
                var snap = await _db.Child("rooms").Child(roomId).GetValueAsync();
                if (!snap.Exists) break;

                long expiryAt = 0;
                var exp = snap.Child("expiryAt").Value;
                if (exp != null) long.TryParse(exp.ToString(), out expiryAt);

                if (expiryAt > 0 && NowMs() >= expiryAt)
                {
                    try { await _db.Child("rooms").Child(roomId).RemoveValueAsync(); } catch { }
                    if (roomId == CurrentRoomId) ResetLocal();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[RoomService] AutoDeleteIfExpired error: " + ex.Message);
        }
    }
}
