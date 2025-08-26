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

    public async void SaveName()
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

        // 1) Ghi tên lên Realtime DB (giữ nguyên luồng cũ)
        dataBaseManager.UpdateUserName(userId, playerName);

        // 2) Đồng bộ sang FirebaseAuth.DisplayName (optional nhưng tốt)
        try
        {
            var profile = new UserProfile { DisplayName = playerName };
            await firebaseUser.UpdateUserProfileAsync(profile);
            await firebaseUser.ReloadAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Không thể cập nhật DisplayName: " + ex.Message);
        }

        // 3) Phát sự kiện + lưu cache để lần sau vào game có tên ngay
        PlayerName.Set(playerName);
        PlayerNameInit.SaveCache(playerName);

        // 4) Cập nhật UI cục bộ
        UpdateName(playerName);
    }

    private void Start()
    {
        if (dataBaseManager == null)
        {
            dataBaseManager = FindObjectOfType<FireBaseDataBaseManager>();
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

        // Load tên từ DB và cập nhật đồng bộ
        dataBaseManager.LoadUserName(firebaseUser.UserId, UpdateName);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            if (nameInputPanel) nameInputPanel.SetActive(true);
            return;
        }

        if (nameText != null) nameText.text = name;
        if (nameTextUpdate != null) nameTextUpdate.text = name;

        PlayerName.Set(name);
        PlayerNameInit.SaveCache(name);

        // Đảm bảo DisplayName khớp (fire & forget)
        _ = EnsureAuthDisplayName(name);
    }

    async System.Threading.Tasks.Task EnsureAuthDisplayName(string wanted)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null || user.DisplayName == wanted) return;

        try
        {
            var profile = new UserProfile { DisplayName = wanted };
            await user.UpdateUserProfileAsync(profile);
            await user.ReloadAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("EnsureAuthDisplayName failed: " + ex.Message);
        }
    }
}
