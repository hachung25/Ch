using UnityEngine;
using TMPro;
using Firebase.Auth;

public class NameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nameTextUpdate;

    [Header("Firebase")]
    public FireBaseDataBaseManager dataBaseManager;

    public GameObject nameInputPanel;

    public void SaveName()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Tên không được để trống!");
            return;
        }

        FirebaseUser firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (firebaseUser == null)
        {
            Debug.LogError("Chưa đăng nhập vào Firebase!");
            return;
        }

        string userId = firebaseUser.UserId;

        // Gọi hàm ghi tên lên Firebase
        dataBaseManager.UpdateUserName(userId, playerName);

        // Cập nhật UI
        UpdateName(playerName);
    }

    private void Start()
    {
        // Tự động gán nếu quên kéo trong Inspector
        if (dataBaseManager == null)
        {
            dataBaseManager = FindObjectOfType<FireBaseDataBaseManager>();
            if (dataBaseManager == null)
            {
                Debug.LogError("Không tìm thấy FireBaseDataBaseManager trong scene!");
                return;
            }
        }

        // Load tên nếu người chơi đã đăng nhập
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
         
            nameInputPanel.SetActive(true);

            return;
        }
        
        if (nameText != null) nameText.text = name;
        if (nameTextUpdate != null) nameTextUpdate.text = name;
    }
}