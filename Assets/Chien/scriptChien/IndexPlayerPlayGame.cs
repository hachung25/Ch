using System;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class IndexPlayerPlayGame : MonoBehaviour
{
    public static int PlayerHealthValue { get; private set; }
    public static int PlayerDamageValue { get; private set; }

    public static DatabaseReference reference;
    public static string userId;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển scene
    }

    private void Start()
    {
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

                PlayerHealthValue = health;
                PlayerDamageValue = damage;

                Debug.Log($"Đã tải Health: {health}, Damage: {damage}");
            }
        });
    }
}
