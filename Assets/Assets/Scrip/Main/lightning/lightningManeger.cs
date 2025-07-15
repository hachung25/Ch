using UnityEngine;
using System;

public class lightningManeger : MonoBehaviour
{
    private const string GoldKey = "lightning";
    
    // Sự kiện gọi khi vàng thay đổi
    public static event Action<int> OnlightningChanged;

    // Thêm vàng
    public static void Addlightning(int amount)
    {
        int currentGold = GetLightning();
        currentGold += amount;
        SetGold(currentGold);
    }

    // Trừ vàng
    public static bool Spendlightning(int amount)
    {
        int currentGold = GetLightning();
        if (currentGold >= amount)
        {
            currentGold -= amount;
            SetGold(currentGold);
            return true;
        }
        return false;
    }

    // Lấy số vàng hiện tại
    public static int GetLightning()
    {
        return PlayerPrefs.GetInt(GoldKey, 0);
    }

    // Đặt số vàng (nội bộ)
    private static void SetGold(int amount)
    {
        PlayerPrefs.SetInt(GoldKey, amount);
        PlayerPrefs.Save();
        OnlightningChanged?.Invoke(amount);
    }

    // Reset toàn bộ vàng về 0
    public static void ResetGold()
    {
        PlayerPrefs.DeleteKey(GoldKey);
        OnlightningChanged?.Invoke(0);
    }
}
