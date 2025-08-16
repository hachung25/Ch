using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class buttonShowStarr : MonoBehaviour
{
    public GameObject rewardPanel;
    public Button claimButton;   // 👉 Gán button Claim ở Inspector
    public Color claimedColor = Color.gray; // 👉 Màu sau khi nhận thưởng

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {
        rewardPanel.SetActive(false);
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
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
        string today = DateTime.Now.ToString("yyyyMMdd");

        dbRef.Child("users").Child(userId).Child("lastClaimDate").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string lastClaimDate = snapshot.Exists ? snapshot.Value.ToString() : "";

                if (lastClaimDate != today)
                {
                    rewardPanel.SetActive(true);
                    claimButton.interactable = true; // Cho bấm lại
                    claimButton.GetComponent<Image>().color = Color.white; // Reset màu
                }
                else
                {
                    rewardPanel.SetActive(false);
                    claimButton.interactable = false; 
                    claimButton.GetComponent<Image>().color = claimedColor;
                }
            }
            else
            {
                rewardPanel.SetActive(false);
                Debug.LogError("Lỗi khi kiểm tra dữ liệu: " + task.Exception);
            }
        });
    }

    public void OnClaimReward()
    {
        var user = auth.CurrentUser;
        if (user == null) return;

        string userId = user.UserId;
        string today = DateTime.Now.ToString("yyyyMMdd");

        dbRef.Child("users").Child(userId).Child("lastClaimDate").SetValueAsync(today).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                CardSpingManeger.AddCardSping(1);

                // 👉 Đổi màu + vô hiệu hóa nút
                claimButton.interactable = false;
                claimButton.GetComponent<Image>().color = claimedColor;
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
}
