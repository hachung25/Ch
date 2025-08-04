using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutButtonHandler : MonoBehaviour
{
    private FireBaseDataBaseManager databaseManager;

    private void Start()
    {
        databaseManager = FindObjectOfType<FireBaseDataBaseManager>();
    }

    public void OnLogoutClick()
    {
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser != null)
        {
            string userId = currentUser.UserId;

            // Xóa deviceId khỏi onlineStatus
            if (databaseManager != null)
            {
                databaseManager.WriteDataBase("Users/" + userId + "/onlineStatus/deviceId", null);
            }

            FirebaseAuth.DefaultInstance.SignOut();
        }

        SceneManager.LoadScene("LoginTA");

        GameObject watcher = GameObject.Find("OnlineStatusWatcher");
        if (watcher != null) Destroy(watcher);

    }
}
