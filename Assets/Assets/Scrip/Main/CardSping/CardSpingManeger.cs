using UnityEngine;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;


public class CardSpingManeger : MonoBehaviour
{
  
     public static event Action<int> OnCardSpingChanged;

    private static int currentCardSping = 0;

    private static DatabaseReference reference => FirebaseDatabase.DefaultInstance.RootReference;
    private static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    // Thêm Cards
    public static void AddCardSping(int amount)
    {
        currentCardSping += amount;
        SaveCardsToFirebase();
    }

    // Trừ Cards
    public static bool SpendtCardSping(int amount)
    {
        if (currentCardSping >= amount)
        {
            currentCardSping -= amount;
            SaveCardsToFirebase();
            return true;
        }
        return false;
    }

    // Đặt Cards
    public static void SetCardSping(int amount)
    {
        currentCardSping = amount;
        SaveCardsToFirebase();
    }

    // Lấy Cards hiện tại (trong RAM)
    public static int GetCardSping()
    {
        return currentCardSping;
    }

    // Reset Cards
    public static void ResetCardSping()
    {
        currentCardSping = 0;
        SaveCardsToFirebase();
    }

    // Lưu lên Firebase
    private static void SaveCardsToFirebase()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            reference.Child("Users").Child(UserId).Child("CardSping").SetValueAsync(currentCardSping);
        }
        OnCardSpingChanged?.Invoke(currentCardSping);
    }

    // Tải từ Firebase
    public static void LoadCardSpingFromFirebase(Action onDone = null)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            onDone?.Invoke();
            return;
        }

        reference.Child("Users").Child(UserId).Child("CardSping").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int result))
            {
                currentCardSping = result;
            }
            else
            {
                currentCardSping = 0;
            }

            OnCardSpingChanged?.Invoke(currentCardSping);
            onDone?.Invoke();
        });
    }
}
