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
        Debug.Log($"👁️ [Watcher] Bắt đầu theo dõi: {deviceId}");
    }

    private void OnDeviceIdChanged(object sender, ValueChangedEventArgs args)
    {
        string current = args.Snapshot?.Value?.ToString();
        Debug.Log($"[Watcher] Firebase: {current}, local: {deviceId}");
        if (!hasConflict && !string.IsNullOrEmpty(current) && current != deviceId)
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
        cp.onRelogin = () =>
        {
            string newId = FireBaseLoginManager.GetDeviceID();
            FirebaseDatabase.DefaultInstance
                .GetReference($"deviceStatus/{userId}/deviceId")
                .SetValueAsync(newId)
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsCompleted)
                    {
                        Debug.Log("🎉 Đã giành lại quyền!");
                        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
                        SceneManager.LoadScene("LoginTA");
                    }
                });
        };
    }

    private void OnDestroy()
    {
        if (deviceIdRef != null)
            deviceIdRef.ValueChanged -= OnDeviceIdChanged;
    }
}
