using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class buttonShowStarr : MonoBehaviour
{
    public GameObject rewardPanel;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {  rewardPanel.SetActive(false);
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        CheckClaimStatus();
    }

    void CheckClaimStatus()
    {
        var user = auth.CurrentUser;
        if (user == null)
        {
            rewardPanel.SetActive(false); // Không đăng nhập thì ẩn
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
                }
                else
                {
                    rewardPanel.SetActive(false); 
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
            }
            else
            {
                Debug.LogError("Lỗi khi lưu ngày nhận quà: " + task.Exception);
            }
        });
    }

    // 👉 Hàm này để gán vào nút "Tắt"
    public void HidePanel()
    {
        rewardPanel.SetActive(false);
    }
}
