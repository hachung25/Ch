using UnityEngine;
using Firebase.Auth;
public class UnlockMap : MonoBehaviour
{
    public void unlockMap()
    {
        FindObjectOfType<FireBaseDataBaseManager>()?.UnlockMode(FirebaseAuth.DefaultInstance.CurrentUser.UserId);

    }
}
