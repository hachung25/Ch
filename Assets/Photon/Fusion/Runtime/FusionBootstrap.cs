using System.Threading.Tasks;
using Firebase.Database;
using Fusion;
using UnityEngine;

public class FusionBootstrap : MonoBehaviour
{
    public NetworkRunner runnerPrefab;

    async void Start()
    {
        // Lấy roomId đã lưu khi tạo/join
        var roomId = PlayerPrefs.GetString("lastRoomId", "");
        if (string.IsNullOrEmpty(roomId)) { Debug.LogError("Không có RoomId."); return; }

        // Bảo đảm AuthManager tồn tại
        var am = AuthManager.I ?? FindObjectOfType<AuthManager>();
        if (am == null) { Debug.LogError("AuthManager chưa có trong scene."); return; }

        // Chờ có userId (trường hợp AuthManager vừa khởi tạo)
        int guard = 0;
        while (string.IsNullOrEmpty(am.CurrentUserId) && guard++ < 120) // ~6s
        {
            await Task.Delay(50);
        }
        if (string.IsNullOrEmpty(am.CurrentUserId)) { Debug.LogError("Chưa đăng nhập."); return; }

        string uid = am.CurrentUserId;

        // Đọc dữ liệu phòng để xác định Host/Client
        var snap = await FirebaseDatabase.DefaultInstance
            .GetReference($"rooms/{roomId}")
            .GetValueAsync();

        bool isHost = false;
        if (snap.Exists)
        {
            var hostUid = snap.Child("hostUid").Value?.ToString();
            if (!string.IsNullOrEmpty(hostUid)) isHost = (hostUid == uid);

            var me = snap.Child("players").Child(uid);
            if (me.Exists && me.Child("isHost").Value != null)
                isHost |= System.Convert.ToBoolean(me.Child("isHost").Value);

            var hostToken = snap.Child("hostToken").Value?.ToString();
            if (!string.IsNullOrEmpty(hostToken) && hostToken == uid) isHost = true;
        }

        var runner = Instantiate(runnerPrefab);
        var args = new StartGameArgs
        {
            GameMode = isHost ? GameMode.Host : GameMode.Client,
            SessionName = roomId
            // KHÔNG truyền Scene để tránh lệ thuộc API phiên bản
        };

        var result = await runner.StartGame(args);
        if (!result.Ok) Debug.LogError(result.ShutdownReason);
        else Debug.Log(isHost ? "Fusion Host started" : "Fusion Client started");
    }
}
