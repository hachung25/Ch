using UnityEngine;
using TMPro;

public class DailyloginManeger : MonoBehaviour
{
    public DailyloginScriptableojbect dailyloginScriptableojbect;
    public TextMeshProUGUI rewardText;
    public void OnclamDailyReward(int daysindex)
    {
        if (daysindex < dailyloginScriptableojbect.rewards.Count)
        {
            var reward = dailyloginScriptableojbect.rewards[daysindex];
            AddGold(reward.amount);
            rewardText.text = $"Nhận {reward.amount} vàng";
        }
        else
        {
            rewardText.text = "Không có phần thưởng hôm nay";
        }
    }
    void AddGold(int amount)
    {
        GoldManager.AddGold(amount);
        rewardText.text = $"Reward: {amount}";
    }
    public void DeleteData()
    {
        PlayerPrefs.DeleteKey("Gold");
        SaveManeger.ResetDailylogin();
        rewardText.text = "Đã reset dữ liệu!";
    }
}