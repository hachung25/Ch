using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Fusion;
using UnityEngine;

public class FusionRoomStarter : MonoBehaviour
{
    [SerializeField] private Fusion.FusionBootstrap bootstrap;
    [SerializeField] private string roomIdPrefKey = "lastRoomId";
    [SerializeField] private int clientRetryCount = 5;
    [SerializeField] private float clientRetryDelay = 1.0f;

    async void Start()
    {
        if (!bootstrap) bootstrap = FindObjectOfType<Fusion.FusionBootstrap>();
        if (!bootstrap) { Debug.LogError("FusionBootstrap không có trong scene."); return; }

        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) { Debug.LogError("Chưa đăng nhập Firebase."); return; }

        var roomId = PlayerPrefs.GetString(roomIdPrefKey, "");
        if (string.IsNullOrEmpty(roomId)) { Debug.LogError("Không tìm thấy roomId (PlayerPrefs)."); return; }

        DataSnapshot snap;
        try
        {
            snap = await WithTimeout(
                FirebaseDatabase.DefaultInstance.GetReference($"rooms/{roomId}").GetValueAsync(), 10000);
        }
        catch (TimeoutException)
        {
            Debug.LogError("Đọc thông tin phòng bị timeout.");
            return;
        }

        if (snap == null || !snap.Exists) { Debug.LogError("Phòng không tồn tại trên Firebase."); return; }

        string uid = user.UserId;
        bool isHost = false;

        var hostUid = snap.Child("hostUid").Value?.ToString();
        if (!string.IsNullOrEmpty(hostUid) && hostUid == uid) isHost = true;

        var me = snap.Child("players").Child(uid);
        if (me.Exists && me.Child("isHost").Value != null)
            isHost |= Convert.ToBoolean(me.Child("isHost").Value);

        // Dùng roomId làm SessionName
        bootstrap.DefaultRoomName = roomId;

        // Khởi chạy Fusion
        if (isHost)
        {
            try
            {
                bootstrap.StartHost();
                Debug.Log("[Fusion] Started as Host.");
            }
            catch (Exception e)
            {
                Debug.LogError("[Fusion] StartHost error: " + e.Message);
            }
        }
        else
        {
            // Client: thử nhiều lần phòng hợp lệ (host có thể khởi động chậm)
            bool ok = false;
            for (int i = 0; i < clientRetryCount && !ok; i++)
            {
                try
                {
                    bootstrap.StartClient();
                    ok = true;
                    Debug.Log("[Fusion] Started as Client.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Fusion] StartClient failed (attempt {i + 1}/{clientRetryCount}): {e.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(clientRetryDelay));
                }
            }
            if (!ok) Debug.LogError("[Fusion] StartClient failed after retries.");
        }
    }

    static async Task<T> WithTimeout<T>(Task<T> task, int ms)
    {
        var finished = await Task.WhenAny(task, Task.Delay(ms));
        if (finished == task) return task.Result;
        throw new TimeoutException();
    }
}
