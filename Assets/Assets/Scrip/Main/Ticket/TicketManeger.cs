using UnityEngine;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class TicketManeger : MonoBehaviour
{
     public static event Action<int> OnTicketChanged;

    private static int currentTicket = 0;

    private static DatabaseReference reference => FirebaseDatabase.DefaultInstance.RootReference;
    private static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    // Thêm Cards
    public static void AddTicket(int amount)
    {
        currentTicket += amount;
        SaveCardsToFirebase();
    }

    // Trừ Cards
    public static bool SpendTicket(int amount)
    {
        if (currentTicket >= amount)
        {
            currentTicket -= amount;
            SaveCardsToFirebase();
            return true;
        }
        return false;
    }

    // Đặt Cards
    public static void SetTicket(int amount)
    {
        currentTicket = amount;
        SaveCardsToFirebase();
    }

    // Lấy Cards hiện tại (trong RAM)
    public static int GetTicket()
    {
        return currentTicket;
    }

    // Reset Cards
    public static void ResetTicket()
    {
        currentTicket = 0;
        SaveCardsToFirebase();
    }

    // Lưu lên Firebase
    private static void SaveCardsToFirebase()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            reference.Child("Users").Child(UserId).Child("Ticket").SetValueAsync(currentTicket);
        }
        OnTicketChanged?.Invoke(currentTicket);
    }

    // Tải từ Firebase
    public static void LoadTicketFromFirebase(Action onDone = null)
    {
        if (string.IsNullOrEmpty(UserId))
        {
            onDone?.Invoke();
            return;
        }

        reference.Child("Users").Child(UserId).Child("Ticket").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int result))
            {
                currentTicket = result;
            }
            else
            {
                currentTicket = 0;
            }

            OnTicketChanged?.Invoke(currentTicket);
            onDone?.Invoke();
        });
    }
}
