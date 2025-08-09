using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineStatusWatcher : MonoBehaviour
{
    private string userId, deviceId;
    private DatabaseReference deviceIdRef;
    private bool hasConflict = false;
    public GameObject conflictPopupPrefab;

    public void StartWatching(string _userId, string _deviceId)
    {
        userId = _userId;
        deviceId = _deviceId;

        deviceIdRef = FirebaseDatabase.DefaultInstance
            .GetReference($"deviceStatus/{userId}/deviceId");
        deviceIdRef.ValueChanged += OnDeviceIdChanged;

        Debug.Log($"👁️ [Watcher] Bắt đầu theo dõi: localDeviceId={deviceId}");
    }

    // Cho phép dừng theo dõi; gọi khi Logout/SignOut hoặc rời app
    public void StopWatching(bool destroy = true)
    {
        if (deviceIdRef != null)
        {
            deviceIdRef.ValueChanged -= OnDeviceIdChanged;
            deviceIdRef = null;
        }
        hasConflict = false;
        Debug.Log("👁️ [Watcher] Đã dừng theo dõi.");
        if (destroy) Destroy(gameObject);
    }

    private void OnDeviceIdChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
    {
        var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        if (auth == null || auth.CurrentUser == null) return; // đã logout thì thôi
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LoginTA") return;

        string current = args.Snapshot?.Value?.ToString();
        Debug.Log($"[Watcher] Firebase: {current}, local: {deviceId}");

        if (string.IsNullOrEmpty(current)) { hasConflict = false; return; } // server clear → bỏ qua
        if (current == deviceId) { hasConflict = false; return; }           // vẫn là mình → bỏ qua

        if (!hasConflict)
        {
            hasConflict = true;
            ShowConflictPopup();
        }
    }


    private void ShowConflictPopup()
    {
        if (conflictPopupPrefab == null)
        {
            Debug.LogError("Popup prefab không gán!");
            return;
        }

        GameObject popup = Instantiate(conflictPopupPrefab);
        DontDestroyOnLoad(popup);
        var cp = popup.GetComponent<ConflictPopup>();

        // YÊU CẦU MỚI:
        // Khi bấm "Đăng nhập lại" trên thiết bị đang bị đá ra,
        // → chỉ đăng xuất cục bộ và về LoginTA, KHÔNG ghi deviceId lên Firebase.
        cp.onRelogin = () =>
        {
            Debug.Log("🔒 Relogin pressed on losing device → local SignOut only.");

            // 1) Ngừng theo dõi để không còn nhận event
            StopWatching(destroy: false);

            // 2) Đăng xuất cục bộ
            try
            {
                Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SignOut failed: {e.Message}");
            }

            // 3) Về scene LoginTA
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginTA");

            // 4) Hủy watcher này
            Destroy(gameObject);
        };
    }


    private void OnDestroy()
    {
        if (deviceIdRef != null)
            deviceIdRef.ValueChanged -= OnDeviceIdChanged;
    }


}
