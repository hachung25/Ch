using UnityEngine;
using System;

public static class GoldManager
{
    private const string GoldKey = "Gold";
    
    // Sự kiện gọi khi vàng thay đổi
    public static event Action<int> OnGoldChanged;

    // Thêm vàng
    public static void AddGold(int amount)
    {
        int currentGold = GetGold();
        currentGold += amount;
        SetGold(currentGold);
    }

    // Trừ vàng
    public static bool SpendGold(int amount)
    {
        int currentGold = GetGold();
        if (currentGold >= amount)
        {
            currentGold -= amount;
            SetGold(currentGold);
            return true;
        }
        return false;
    }

    // Lấy số vàng hiện tại
    public static int GetGold()
    {
        return PlayerPrefs.GetInt(GoldKey, 0);
    }

    // Đặt số vàng (nội bộ)
    private static void SetGold(int amount)
    {
        PlayerPrefs.SetInt(GoldKey, amount);
        PlayerPrefs.Save();
        OnGoldChanged?.Invoke(amount);
    }

    // Reset toàn bộ vàng về 0
    public static void ResetGold()
    {
        PlayerPrefs.DeleteKey(GoldKey);
        OnGoldChanged?.Invoke(0);
    }
}