using UnityEngine;

public class testgold : MonoBehaviour
{
    public void Hehe()
    {
        GoldManager.AddGold(50);
        Debug.Log("🎉 Đã nhận 50 vàng từ quái! Tổng vàng: " + GoldManager.GetGold());
    }
}
