using UnityEngine;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public static class GoldManager
{
    private static int currentGold = 0;

    public static event Action<int> OnGoldChanged;

    private static DatabaseReference reference => FirebaseDatabase.DefaultInstance.RootReference;
    private static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    // Thêm vàng
    public static void AddGold(int amount)
    {
        currentGold += amount;
        SaveGoldToFirebase();
    }

    // Trừ vàng
    public static bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            SaveGoldToFirebase();
            return true;
        }
        return false;
    }

    // Lấy vàng hiện tại
    public static int GetGold()
    {
        return currentGold;
    }

    // Đặt số vàng
    public static void SetGold(int amount)
    {
        currentGold = amount;
        SaveGoldToFirebase();
    }

    // Reset vàng
    public static void ResetGold()
    {
        currentGold = 0;
        SaveGoldToFirebase();
    }

    // Lưu vàng lên Firebase
    private static void SaveGoldToFirebase()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            reference.Child("Users").Child(UserId).Child("Gold").SetValueAsync(currentGold);
        }
        OnGoldChanged?.Invoke(currentGold);
    }

    // Tải vàng từ Firebase
    public static void LoadGoldFromFirebase(Action onDone = null)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            onDone?.Invoke();
            return;
        }

        reference.Child("Users").Child(UserId).Child("Gold").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int result))
            {
                currentGold = result;
            }
            else
            {
                currentGold = 0;
            }

            OnGoldChanged?.Invoke(currentGold);
            onDone?.Invoke();
        });
    }
}
