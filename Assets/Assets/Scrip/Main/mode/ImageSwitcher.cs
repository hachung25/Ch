using System;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class ImageSwitcher : MonoBehaviour
{
    public bool Modee;           // cờ kiểm tra mở mode 2
    public GameObject lockImage; // Image báo khóa (kéo vào từ Inspector)
    public GameObject PanelMode1;
    public GameObject PanelMode2;

    void Start()
    {
        UpData();
    }

    // Load trạng thái Mode2 từ Firebase
    public void UpData()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            FireBaseDataBaseManager db = FindObjectOfType<FireBaseDataBaseManager>();
            if (db != null)
            {
                db.LoadMode(userId, ApplyMode); // callback khi có mode
            }
            else
            {
                Debug.LogWarning("Không tìm thấy FireBaseDataBaseManager.");
            }
        }
        else
        {
            Debug.LogWarning("User chưa đăng nhập.");
        }
    }

    private void ApplyMode(bool mode)
    {
        Modee = mode;
        CheckMode2(); // Debug + cập nhật image ngay khi load xong
    }

    // === BUTTON DUY NHẤT GỌI HÀM NÀY ===
    public void OnClickCheckMode2()
    {
        CheckMode2();
    }

    private void CheckMode2()
    {
        if (Modee)
        {
            Debug.Log("Mode 2 đã mở");
            if (lockImage != null) lockImage.SetActive(false);
        }
        else
        {
            Debug.Log("Mode 2 chưa mở");
            if (lockImage != null) lockImage.SetActive(true);
        }
    }

    public void LoadMode2()
    {
        if (Modee)
        {
            SceneManager.LoadScene("Mode2");
            PanelMode2.SetActive(false);
            PanelMode1.SetActive(false);
            
        }
    }
}