using UnityEngine;
using System;

public class CardsManeger : MonoBehaviour
{
    private const string Key = "Cards";
    
    // Sự kiện gọi khi vàng thay đổi
    public static event Action<int> OnCardsChanged;

    // Thêm vàng
    public static void AddCards(int amount)
    {
        int currentGold = GetCards();
        currentGold += amount;
        SetCards(currentGold);
    }

    // Trừ vàng
    public static bool SpendCards(int amount)
    {
        int currentGold = GetCards();
        if (currentGold >= amount)
        {
            currentGold -= amount;
            SetCards(currentGold);
            return true;
        }
        return false;
    }

    // Lấy số vàng hiện tại
    public static int GetCards()
    {
        return PlayerPrefs.GetInt(Key, 0);
    }

    // Đặt số vàng (nội bộ)
    private static void SetCards(int amount)
    {
        PlayerPrefs.SetInt(Key, amount);
        PlayerPrefs.Save();
        OnCardsChanged?.Invoke(amount);
    }

    // Reset toàn bộ vàng về 0
    public static void ResetCards()
    {
        PlayerPrefs.DeleteKey(Key);
        OnCardsChanged?.Invoke(0);
    }
}
