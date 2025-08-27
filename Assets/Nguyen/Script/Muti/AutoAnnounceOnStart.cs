using System.Collections;
using Firebase.Auth;
using Fusion;
using UnityEngine;

public class AutoAnnounceOnStart : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Đợi Firebase đăng nhập xong
        float t = 0f;
        while (FirebaseAuth.DefaultInstance.CurrentUser == null && t < 10f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Đợi NetworkRunner xuất hiện
        NetworkRunner runner = null;
        t = 0f;
        while ((runner = FindObjectOfType<NetworkRunner>()) == null && t < 10f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Đợi SceneObject 'Announcer' sẵn sàng
        JoinAnnounce announcer = null;
        t = 0f;
        while ((announcer = FindObjectOfType<JoinAnnounce>()) == null && t < 10f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        var uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        int charIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        if (!string.IsNullOrEmpty(uid) && announcer != null)
        {
            // Gửi RPC lên server
            announcer.RPC_Announce(uid, charIndex);
            Debug.Log($"[AutoAnnounceOnStart] Announced uid={uid}, char={charIndex}");
        }
        else
        {
            Debug.LogWarning("[AutoAnnounceOnStart] Announce failed (uid/announcer missing).");
        }
    }
}
