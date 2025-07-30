using System;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class IndexPlayer : MonoBehaviour
{
    public TextMeshProUGUI text1Health;
    public TextMeshProUGUI text2Dame;

    private DatabaseReference reference;
    private string userId;

     void Start()
    {
        if(text1Health)
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        updateIndex();
    }

    public void updateIndex()
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
                Debug.LogError("Lỗi khi tải dữ liệu chỉ số.");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                int health = 0;
                int damage = 0;

                if (snapshot.HasChild("Health"))
                    int.TryParse(snapshot.Child("Health").Value.ToString(), out health);
                if (snapshot.HasChild("Damage"))
                    int.TryParse(snapshot.Child("Damage").Value.ToString(), out damage);

                text1Health.text = health.ToString();
                text2Dame.text = damage.ToString();
            }
        });
    }
}