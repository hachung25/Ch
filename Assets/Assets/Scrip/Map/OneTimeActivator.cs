using UnityEngine;
using Firebase.Auth; // cần Firebase

public class OneTimeActivator : MonoBehaviour
{
    [Header("GameObject chỉ định (kéo vào đây)")]
    public GameObject targetObject;
    public GameObject Playerrr;

    private string saveKey = "MyObjectActivated";

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("⚠ Chưa gán GameObject nào vào OneTimeActivator!");
            return;
        }

        // Lấy UserId hiện tại từ Firebase
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("⚠ Không tìm thấy userId (chưa đăng nhập?)");
            return;
        }

        // Ghép key với userId để phân biệt từng account
        string fullKey = saveKey + "_" + userId;

        // Kiểm tra trạng thái đã lưu cho tài khoản này
        if (PlayerPrefs.GetInt(fullKey, 0) == 0)
        {
            // Lần đầu tiên -> bật
            targetObject.SetActive(true);
            Debug.Log("🟢 Bật lần đầu cho user: " + userId);

            // Lưu trạng thái
            PlayerPrefs.SetInt(fullKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            // Các lần sau -> ẩn target, bật Player
            targetObject.SetActive(false);
            Playerrr.SetActive(true);
            Debug.Log("🔵 Đã từng bật trước đó cho user: " + userId);
        }
    }
}