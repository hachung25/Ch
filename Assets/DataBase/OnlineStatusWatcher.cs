using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineStatusWatcher : MonoBehaviour
{
    private string userId;
    private string deviceId;
    private DatabaseReference deviceIdRef;
    private bool hasConflict = false;
    public GameObject conflictPopupPrefab;

    public void StartWatching(string _userId, string _deviceId)
    {
        userId = _userId;
        deviceId = _deviceId;

        deviceIdRef = FirebaseDatabase.DefaultInstance
            .GetReference("Users")
            .Child(userId)
            .Child("onlineStatus")
            .Child("deviceId");

        deviceIdRef.ValueChanged += OnDeviceIdChanged;
    }

    private void OnDeviceIdChanged(object sender, ValueChangedEventArgs args)
    {
        if (!args.Snapshot.Exists || args.Snapshot.Value == null) return;

        string currentOnlineDevice = args.Snapshot.Value.ToString();

        if (currentOnlineDevice != deviceId && !hasConflict)
        {
            Debug.LogWarning("⚠️ Phát hiện đăng nhập ở thiết bị khác!");

            hasConflict = true;
            ShowConflictPopup();
        }
    }

    private void ShowConflictPopup()
    {
        if (conflictPopupPrefab == null)
        {
            Debug.LogError("❌ conflictPopupPrefab = null! Chưa gán prefab trong FireBaseLoginManager.");
            return;
        }

        GameObject popup = Instantiate(conflictPopupPrefab);
        DontDestroyOnLoad(popup);

        ConflictPopup popupScript = popup.GetComponent<ConflictPopup>();
        if (popupScript == null)
        {
            Debug.LogError("❌ ConflictPopup.cs script không gắn vào prefab!");
            return;
        }

        popupScript.onRelogin = () =>
        {
            Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
            SceneManager.LoadScene("LoginTA");
        };
    }


    private void OnDestroy()
    {
        if (deviceIdRef != null)
        {
            deviceIdRef.ValueChanged -= OnDeviceIdChanged;
        }
    }
}
