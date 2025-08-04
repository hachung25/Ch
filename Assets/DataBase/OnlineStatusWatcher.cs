using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineStatusWatcher : MonoBehaviour
{
    private DatabaseReference watchRef;

    public void StartWatching(string userId, string deviceId)
    {
        watchRef = FirebaseDatabase.DefaultInstance
            .GetReference("Users/" + userId + "/onlineStatus/deviceId");

        watchRef.ValueChanged += (object sender, ValueChangedEventArgs e) =>
        {
            if (e.DatabaseError != null || e.Snapshot == null) return;

            string storedDeviceId = e.Snapshot.Value?.ToString();
            if (!string.IsNullOrEmpty(storedDeviceId) && storedDeviceId != deviceId)
            {
                Debug.LogWarning("🔁 Đã phát hiện đăng nhập từ thiết bị khác → văng khỏi game.");
                FirebaseAuth.DefaultInstance.SignOut();
                SceneManager.LoadScene("LoginTA");
            }
        };
    }

    private void OnDestroy()
    {
        if (watchRef != null)
        {
            watchRef.ValueChanged -= null;
        }
    }
}
