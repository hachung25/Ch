using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardFirebaseManager : MonoBehaviour
{
    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    public static RewardFirebaseManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveDate(string key)
    {
        string userId = auth.CurrentUser.UserId;
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        dbRef.Child("users").Child(userId).Child("rewards").Child(key).SetValueAsync(today);
    }

    public void SaveBool(string key)
    {
        string userId = auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("rewards").Child(key).SetValueAsync(true);
    }

    public void GetRewardDate(string key, Action<DateTime?> callback)
    {
        string userId = auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("rewards").Child(key).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                string dateStr = task.Result.Value.ToString();
                if (DateTime.TryParse(dateStr, out DateTime date))
                    callback(date);
                else
                    callback(null);
            }
            else
            {
                callback(null);
            }
        });
    }

    public void GetRewardClaimed(string key, Action<bool> callback)
    {
        string userId = auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("rewards").Child(key).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
                callback(Convert.ToBoolean(task.Result.Value));
            else
                callback(false);
        });
    }
}
