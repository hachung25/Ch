using UnityEngine;
using TMPro;
using Firebase.Auth;

public class NameManagerMuti : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;

    [Header("Firebase")]
    public firebasedataMUti dataBaseManager;

    void Start()
    {
        if (dataBaseManager == null)
        {
            dataBaseManager = FindObjectOfType<firebasedataMUti>();
            if (dataBaseManager == null)
            {
                Debug.LogError("Không tìm thấy FireBaseDataBaseManager trong scene!");
                return;
            }
        }

        FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (firebaseUser == null)
        {
            Debug.LogWarning("Người chơi chưa đăng nhập Firebase.");
            return;
        }

        dataBaseManager.LoadUserName(firebaseUser.UserId, UpdateName);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Tên chưa có trên Firebase.");
            return;
        }

        if (nameText != null) nameText.text = name;
    }
}