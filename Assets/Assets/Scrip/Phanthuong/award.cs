using UnityEngine;
using TMPro;
using System;

public class award : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    private const int cooldownDays = 7;

    public void Dis()
    {
        TryClaimWeeklyReward("Dis");
    }

    public void Fb()
    {
        TryClaimWeeklyReward("Fb");
    }

    private void TryClaimWeeklyReward(string rewardKey)
    {
        string lastClaimKey = $"WeeklyReward_{rewardKey}";
        
        if (!PlayerPrefs.HasKey(lastClaimKey))
        {
            notificationText.text = "🎁 Bạn có thể nhận 50 vàng!";
            GiveReward(lastClaimKey);
        }
        else
        {
            DateTime lastClaim = DateTime.Parse(PlayerPrefs.GetString(lastClaimKey));
            TimeSpan timeSinceClaim = DateTime.Now - lastClaim;

            if (timeSinceClaim.TotalDays >= cooldownDays)
            {
                notificationText.text = "Bạn có thể nhận 50 vàng!";
                GiveReward(lastClaimKey);
            }
            else
            {
                int remainingDays = cooldownDays - Mathf.FloorToInt((float)timeSinceClaim.TotalDays);
                notificationText.text = $"Bạn đã nhận rồi.\nQuay lại sau {remainingDays} ngày nữa.";
            }
        }
    }

    private void GiveReward(string claimKey)
    {
        GoldManager.AddGold(50);
        PlayerPrefs.SetString(claimKey, DateTime.Now.ToString());
        PlayerPrefs.Save();

        notificationText.text = "Đã nhận 50 vàng.\nTuần sau quay lại!";
    }

    public void Deletedays()
    {
        PlayerPrefs.DeleteKey("WeeklyReward_Dis");
        PlayerPrefs.DeleteKey("WeeklyReward_Fb");
        PlayerPrefs.Save();
    }
}