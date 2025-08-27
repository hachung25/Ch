using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class buttonShowStarr : MonoBehaviour
{
    public GameObject rewardPanel;
    public Button claimButton;
    public Color claimedColor = Color.gray;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private string today;

    void Start()
    {
        rewardPanel.SetActive(false);
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        today = DateTime.Now.ToString("yyyyMMdd");
        CheckClaimStatus();
    }

    void CheckClaimStatus()
    {
        var user = auth.CurrentUser;
        if (user == null)
        {
            rewardPanel.SetActive(false);
            return;
        }

        string userId = user.UserId;
        string localDate = PlayerPrefs.GetString("lastClaimDate", "");
        string localUser = PlayerPrefs.GetString("lastClaimUserId", "");

        // Nếu login bằng account khác -> xoá dữ liệu cũ
        if (!string.IsNullOrEmpty(localUser) && localUser != userId)
        {
            ClearRewardLocalData();
            localDate = "";
        }

        // Nếu local lưu ngày hôm nay -> chặn luôn
        if (localDate == today)
        {
            rewardPanel.SetActive(false);
            claimButton.interactable = false;
            claimButton.GetComponent<Image>().color = claimedColor;
            return;
        }

        // Nếu local chưa có -> check Firebase
        dbRef.Child("users").Child(userId).Child("lastClaimDate")
            .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string lastClaimDate = snapshot.Exists ? snapshot.Value.ToString() : "";

                if (lastClaimDate != today)
                {
                    rewardPanel.SetActive(true);
                    claimButton.interactable = true;
                    claimButton.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    rewardPanel.SetActive(false);
                    claimButton.interactable = false;
                    claimButton.GetComponent<Image>().color = claimedColor;

                    // Đồng bộ local
                    PlayerPrefs.SetString("lastClaimDate", today);
                    PlayerPrefs.SetString("lastClaimUserId", userId);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Debug.LogError("Lỗi khi kiểm tra dữ liệu: " + task.Exception);
            }
        });
    }

    public void OnClaimReward()
    {
        var user = auth.CurrentUser;
        if (user == null) return;

        string userId = user.UserId;

        dbRef.Child("users").Child(userId).Child("lastClaimDate")
            .SetValueAsync(today).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // 👉 Thưởng
                CardSpingManeger.AddCardSping(1);

                // Cập nhật UI
                claimButton.interactable = false;
                claimButton.GetComponent<Image>().color = claimedColor;
                rewardPanel.SetActive(false);

                // Lưu local
                PlayerPrefs.SetString("lastClaimDate", today);
                PlayerPrefs.SetString("lastClaimUserId", userId);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("Lỗi khi lưu ngày nhận quà: " + task.Exception);
            }
        });
    }

    public void HidePanel()
    {
        rewardPanel.SetActive(false);
    }

    // 👉 Hàm xoá dữ liệu local (gọi khi đăng nhập tài khoản khác hoặc debug)
    public static void ClearRewardLocalData()
    {
        PlayerPrefs.DeleteKey("lastClaimDate");
        PlayerPrefs.DeleteKey("lastClaimUserId");
        PlayerPrefs.Save();
    }
}
