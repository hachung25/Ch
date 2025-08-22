using UnityEngine;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class LevelManager : MonoBehaviour
{
    private static int currentLevel = 0;

    public static event Action<int> OnLevelChanged;

    private static DatabaseReference reference => FirebaseDatabase.DefaultInstance.RootReference;
    private static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    private void Start()
    {
        // Khi game chạy, load level từ Firebase
        LoadLevelFromFirebase();
    }

    // Thêm level
    public static void AddLevel(int amount)
    {
        currentLevel += amount;
        OnLevelChanged?.Invoke(currentLevel);
        SaveLevelToFirebase();
    }

    // Lấy level hiện tại
    public static int GetLevel()
    {
        return currentLevel;
    }

    // Đặt level
    public static void SetLevel(int amount)
    {
        currentLevel = amount;
        OnLevelChanged?.Invoke(currentLevel); // UI update ngay
        SaveLevelToFirebase(); // lưu Firebase
    }

    // Reset level
    public static void ResetLevel()
    {
        currentLevel = 0;
        OnLevelChanged?.Invoke(currentLevel);
        SaveLevelToFirebase();
    }

    // Lưu level lên Firebase
    private static void SaveLevelToFirebase()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            reference.Child("Users").Child(UserId).Child("Level").SetValueAsync(currentLevel);
        }
    }

    // Tải level từ Firebase
    public static void LoadLevelFromFirebase(Action onDone = null) 
    {
        if (string.IsNullOrEmpty(UserId))
        {
            onDone?.Invoke();
            return;
        }

        reference.Child("Users").Child(UserId).Child("Level").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int result))
            {
                currentLevel = result;
            }
            else
            {
                currentLevel = 0;
            }

            OnLevelChanged?.Invoke(currentLevel); // update UI ngay khi load
            onDone?.Invoke();
        });
    }
}
