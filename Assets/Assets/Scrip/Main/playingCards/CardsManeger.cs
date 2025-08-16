using UnityEngine;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class CardsManeger : MonoBehaviour
{
    public static event Action<int> OnCardsChanged;

    private static int currentCards = 0;

    private static DatabaseReference reference => FirebaseDatabase.DefaultInstance.RootReference;
    private static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    // Thêm Cards   
    public static void AddCards(int amount)
    {
        currentCards += amount;
        SaveCardsToFirebase();
    }

    // Trừ Cards
    public static bool SpendCards(int amount)
    {
        if (currentCards >= amount)
        {
            currentCards -= amount;
            SaveCardsToFirebase();
            return true;
        }
        return false;
    }

    // Đặt Cards
    public static void SetCards(int amount)
    {
        currentCards = amount;
        SaveCardsToFirebase();
    }

    // Lấy Cards hiện tại (trong RAM)
    public static int GetCards()
    {
        return currentCards;
    }

    // Reset Cards
    public static void ResetCards()
    {
        currentCards = 0;
        SaveCardsToFirebase();
    }

    // Lưu lên Firebase
    private static void SaveCardsToFirebase()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            reference.Child("Users").Child(UserId).Child("Cards").SetValueAsync(currentCards);
        }
        OnCardsChanged?.Invoke(currentCards);
    }

    // Tải từ Firebase
    public static void LoadCardsFromFirebase(Action onDone = null)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            onDone?.Invoke();
            return;
        }

        reference.Child("Users").Child(UserId).Child("Cards").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int result))
            {
                currentCards = result;
            }
            else
            {
                currentCards = 0;
            }

            OnCardsChanged?.Invoke(currentCards);
            onDone?.Invoke();
        });
    }
}
