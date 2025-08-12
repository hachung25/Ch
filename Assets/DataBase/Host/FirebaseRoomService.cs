using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseRoomService : MonoBehaviour
{
    private DatabaseReference _root;
    public static FirebaseRoomService I;

    void Awake()
    {
        I = this;
        _root = FirebaseDatabase.DefaultInstance.RootReference;
    }

    string RoomsPath => "rooms";
    string PresencePath => "userPresence";

    // Tạo phòng
    public async Task<string> CreateRoomAsync(string hostUserId, string hostToken, string hostName)
    {
        string roomId = GenerateRoomId();
        var roomRef = _root.Child(RoomsPath).Child(roomId);

        var data = new Dictionary<string, object> {
            { "hostUserId", hostUserId },
            { "hostToken", hostToken ?? "" },
            { "hostGame", true },
            { "status", "lobby" },
            { "sceneToLoad", "" },
            { "createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var players = new Dictionary<string, object> {
            { hostUserId, new Dictionary<string, object> {
                { "name", hostName },
                { "joinedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "isHost", true }
            } }
        };
        data["players"] = players;

        await roomRef.SetValueAsync(data);
        await BindPresence(hostUserId, roomId);

        // Lưu roomId
        PlayerPrefs.SetString("room_id", roomId);
        PlayerPrefs.Save();
        return roomId;
    }

    // Join phòng
    public async Task<bool> JoinRoomAsync(string roomId, string userId, string displayName)
    {
        var roomRef = _root.Child(RoomsPath).Child(roomId);
        var snap = await roomRef.GetValueAsync();
        if (!snap.Exists || snap.Child("status").Value?.ToString() == "closed")
            return false;

        await roomRef.Child("players").Child(userId).SetValueAsync(new Dictionary<string, object> {
            { "name", displayName },
            { "joinedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "isHost", false }
        });

        await BindPresence(userId, roomId);

        PlayerPrefs.SetString("room_id", roomId);
        PlayerPrefs.Save();
        return true;
    }

    // Danh sách phòng đang lobby
    public void ListenLobbyRooms(Action<List<RoomInfo>> onUpdate)
    {
        var query = _root.Child(RoomsPath).OrderByChild("status").EqualTo("lobby");
        query.ValueChanged += (s, e) =>
        {
            var list = new List<RoomInfo>();
            if (e.Snapshot.Exists)
            {
                foreach (var child in e.Snapshot.Children)
                {
                    string roomId = child.Key;
                    string host = child.Child("hostUserId").Value?.ToString();
                    int count = (int)child.Child("players").ChildrenCount;
                    list.Add(new RoomInfo { roomId = roomId, hostUserId = host, playerCount = count });
                }
            }
            onUpdate?.Invoke(list);
        };
    }

    // Lắng nghe chuyển cảnh
    public void ListenSceneLoad(string roomId, Action<string> onSceneToLoad)
    {
        _root.Child(RoomsPath).Child(roomId).Child("sceneToLoad")
            .ValueChanged += (s, e) =>
            {
                if (!e.Snapshot.Exists) return;
                string scene = e.Snapshot.Value?.ToString();
                if (!string.IsNullOrEmpty(scene)) onSceneToLoad?.Invoke(scene);
            };

        _root.Child(RoomsPath).Child(roomId).Child("status")
            .ValueChanged += (s, e) =>
            {
                // có thể dùng nếu cần chặn/đồng bộ UI theo trạng thái
            };
    }

    // Host bấm Start
    public async Task HostStartGame(string roomId, string sceneName)
    {
        var roomRef = _root.Child(RoomsPath).Child(roomId);
        await roomRef.UpdateChildrenAsync(new Dictionary<string, object> {
            { "status", "loading" },
            { "sceneToLoad", sceneName }
        });
    }

    // Client/Host rời phòng
    public async Task LeaveRoom(string roomId, string userId, bool isHost)
    {
        var roomRef = _root.Child(RoomsPath).Child(roomId);
        if (isHost)
        {
            // Host thoát: xóa toàn phòng
            await roomRef.RemoveValueAsync();
        }
        else
        {
            await roomRef.Child("players").Child(userId).RemoveValueAsync();
        }

        // Xóa roomId local
        PlayerPrefs.DeleteKey("room_id");
        PlayerPrefs.Save();
    }

    // Presence và tự remove khi disconnect
    async Task BindPresence(string userId, string roomId)
    {
        var presenceRef = _root.Child(PresencePath).Child(userId);
        await presenceRef.UpdateChildrenAsync(new Dictionary<string, object> {
            { "online", true },
            { "lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "roomId", roomId }
        });
        // Xoá giá trị khi disconnect
        var playerRefInRoom = _root
            .Child(RoomsPath)
            .Child(roomId)
            .Child("players")
            .Child(userId);

        playerRefInRoom.OnDisconnect().RemoveValue();

        // Update khi disconnect
        var presenceRef = _root.Child(PresencePath).Child(userId);
        presenceRef.OnDisconnect().UpdateChildrenAsync(new Dictionary<string, object> {
    { "online", false },
    { "lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
    { "roomId", "" }
});

    }

    string GenerateRoomId()
    {
        // 6-8 ký tự ngắn dễ nhập
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        System.Random r = new System.Random();
        char[] buf = new char[6];
        for (int i = 0; i < buf.Length; i++)
            buf[i] = chars[r.Next(chars.Length)];
        return new string(buf);
    }

    [Serializable]
    public class RoomInfo { public string roomId; public string hostUserId; public int playerCount; }

    public void ListenPlayers(string roomId, Action<List<PlayerInfo>> onUpdate)
    {
        var refPlayers = FirebaseDatabase.DefaultInstance
                       .RootReference.Child("rooms").Child(roomId).Child("players");
        refPlayers.ValueChanged += (s, e) =>
        {
            var list = new List<PlayerInfo>();
            if (e.Snapshot.Exists)
            {
                foreach (var ch in e.Snapshot.Children)
                {
                    list.Add(new PlayerInfo
                    {
                        userId = ch.Key,
                        name = ch.Child("name").Value?.ToString(),
                        isHost = ch.Child("isHost").Value?.ToString() == "True"
                    });
                }
            }
            onUpdate?.Invoke(list);
        };
    }

    public class PlayerInfo { public string userId; public string name; public bool isHost; }

}
