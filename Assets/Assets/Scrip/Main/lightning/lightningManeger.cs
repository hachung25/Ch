using UnityEngine;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public static class lightningManeger
{
    private static int currentLightning = 0;

    public static event Action<int> OnLightningChanged;

    private static DatabaseReference reference => FirebaseDatabase.DefaultInstance.RootReference;
    private static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    public static void AddLightning(int amount)
    {
        currentLightning += amount;
        SaveLightningToFirebase();
    }

    public static bool SpendLightning(int amount)
    {
        if (currentLightning >= amount)
        {
            currentLightning -= amount;
            SaveLightningToFirebase();
            return true;
        }
        return false;
    }

    public static int GetLightning()
    {
        return currentLightning;
    }

    public static void SetLightning(int amount)
    {
        currentLightning = amount;
        SaveLightningToFirebase();
    }

    public static void ResetLightning()
    {
        currentLightning = 0;
        SaveLightningToFirebase();
    }

    private static void SaveLightningToFirebase()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            reference.Child("Users").Child(UserId).Child("Lightning").SetValueAsync(currentLightning);
        }

        OnLightningChanged?.Invoke(currentLightning);
    }

    public static void LoadLightningFromFirebase(Action onDone = null)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            Debug.LogWarning("Chưa đăng nhập.");
            onDone?.Invoke();
            return;
        }

        reference.Child("Users").Child(UserId).Child("Lightning").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int result))
            {
                currentLightning = result;
            }
            else
            {
                currentLightning = 0;
            }

            OnLightningChanged?.Invoke(currentLightning);
            onDone?.Invoke();
        });
    }
}
