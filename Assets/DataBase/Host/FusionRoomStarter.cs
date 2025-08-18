using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class FusionRoomStarter : MonoBehaviour
{
    [SerializeField] private Fusion.FusionBootstrap bootstrap;
    [SerializeField] private string roomIdPrefKey = "lastRoomId";

    async void Start()
    {
        if (!bootstrap) bootstrap = FindObjectOfType<Fusion.FusionBootstrap>();
        if (!bootstrap) { Debug.LogError("FusionBootstrap không có trong scene."); return; }

        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) { Debug.LogError("Chưa đăng nhập Firebase."); return; }

        var roomId = PlayerPrefs.GetString(roomIdPrefKey, "");
        if (string.IsNullOrEmpty(roomId)) { Debug.LogError("Không tìm thấy roomId (PlayerPrefs)."); return; }

        var snap = await FirebaseDatabase.DefaultInstance
                     .GetReference($"rooms/{roomId}")
                     .GetValueAsync();

        if (!snap.Exists) { Debug.LogError("Phòng không tồn tại trên Firebase."); return; }

        string uid = user.UserId;
        bool isHost = false;

        var hostUid = snap.Child("hostUid").Value?.ToString();
        if (!string.IsNullOrEmpty(hostUid) && hostUid == uid) isHost = true;

        var me = snap.Child("players").Child(uid);
        if (me.Exists && me.Child("isHost").Value != null)
            isHost |= System.Convert.ToBoolean(me.Child("isHost").Value);

        var hostToken = snap.Child("hostToken").Value?.ToString();
        if (!string.IsNullOrEmpty(hostToken) && hostToken == uid) isHost = true;

        // Dùng roomId làm SessionName
        bootstrap.DefaultRoomName = roomId;

        // Gọi đúng hàm trên FusionBootstrap mặc định
        if (isHost) bootstrap.StartHost();
        else bootstrap.StartClient();
    }
}
