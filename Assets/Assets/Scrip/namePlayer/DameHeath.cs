using System;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class DameHeath : MonoBehaviour
{
    public TextMeshProUGUI text2Dame;  
    public TextMeshProUGUI text1heath;

    private DatabaseReference reference;
    private string userId;

    private void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        UpdateDameHeath();
    }

    public void UpdateDameHeath()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("Chưa đăng nhập Firebase.");
            return;
        }

        reference.Child("Users").Child(userId).Child("Upgrade").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi khi tải dữ liệu từ Firebase.");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                int heath = 0;
                int dame = 0;

                if (snapshot.HasChild("Health"))
                    int.TryParse(snapshot.Child("Health").Value.ToString(), out heath);

                if (snapshot.HasChild("Damage"))
                    int.TryParse(snapshot.Child("Damage").Value.ToString(), out dame);

                text1heath.text = heath.ToString();
                text2Dame.text = dame.ToString();
            }
        });
    }
}