using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class award : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    private const int cooldownDays = 7;

    // Các hình ảnh thông báo
    public GameObject indicatorGold;
    public GameObject indicatorSet;
    public GameObject indicatorFb;
    public GameObject indicatorDis;

    private void Start()
    {
        notificationText.gameObject.SetActive(false);
        UpdateAllIndicators();
    }

    // 🎁 Phần thưởng có thể lặp lại mỗi 7 ngày
    public void Gold()
    {
        TryClaimWeeklyReward("Gold", isGem: false); // nhận vàng
    }

    public void set()
    {
        TryClaimWeeklyReward("Set", isGem: true); // nhận ngọc
    }

    // 🎁 Phần thưởng chỉ nhận 1 lần duy nhất
    public void Fb()
    {
        TryClaimOneTimeReward("Fb");
    }

    public void Dis()
    {
        TryClaimOneTimeReward("Dis");
    }

    // ✅ Xử lý phần thưởng nhận mỗi 7 ngày
    private void TryClaimWeeklyReward(string rewardKey, bool isGem)
    {
        string lastClaimKey = $"WeeklyReward_{rewardKey}";

        if (!PlayerPrefs.HasKey(lastClaimKey))
        {
            GiveReward(isGem);
            PlayerPrefs.SetString(lastClaimKey, DateTime.Now.ToString());
            PlayerPrefs.Save();
            ShowMessage($"Đã nhận {(isGem ? "set" : "vàng")}thành công!");
        }
        else
        {
            DateTime lastClaim = DateTime.Parse(PlayerPrefs.GetString(lastClaimKey));
            TimeSpan timeSinceClaim = DateTime.Now - lastClaim;

            if (timeSinceClaim.TotalDays >= cooldownDays)
            {
                GiveReward(isGem);
                PlayerPrefs.SetString(lastClaimKey, DateTime.Now.ToString());
                PlayerPrefs.Save();
                ShowMessage($"Đã nhận {(isGem ? "set" : "vàng")} thành công!");
            }
            else
            {
                int remainingDays = cooldownDays - Mathf.FloorToInt((float)timeSinceClaim.TotalDays);
                ShowMessage($"Bạn đã nhận rồi,Quay lại sau {remainingDays} ngày nữa.");
            }
        }

        UpdateAllIndicators();
    }

    // ✅ Xử lý phần thưởng chỉ nhận 1 lần
    private void TryClaimOneTimeReward(string rewardKey)
    {
        string claimKey = $"OneTimeReward_{rewardKey}";

        if (PlayerPrefs.GetInt(claimKey, 0) == 1)
        {
            ShowMessage("Phần thưởng này chỉ nhận 1 lần!");
        }
        else
        {
            GoldManager.AddGold(50);
            PlayerPrefs.SetInt(claimKey, 1);
            PlayerPrefs.Save();
            ShowMessage($"Nhận 50 vàng từ {rewardKey} thành công!");
        }

        UpdateAllIndicators();
    }

    private void GiveReward(bool isGem)
    {
        if (isGem)
            lightningManeger.Addlightning(5); // ngọc
        else
            GoldManager.AddGold(50); // vàng
    }

    // 🧾 Hiện thông báo rồi ẩn sau 2 giây
    private void ShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        notificationText.gameObject.SetActive(false);
    }

    // 🔄 Xóa toàn bộ dữ liệu nhận thưởng
    public void Deletedays()
    {
        PlayerPrefs.DeleteKey("WeeklyReward_Gold");
        PlayerPrefs.DeleteKey("WeeklyReward_Set");
        PlayerPrefs.DeleteKey("OneTimeReward_Fb");
        PlayerPrefs.DeleteKey("OneTimeReward_Dis");
        PlayerPrefs.Save();

        UpdateAllIndicators();
    }

    // ✅ Cập nhật tất cả hình ảnh thông báo
    private void UpdateAllIndicators()
    {
        CheckWeeklyRewardIndicator("Gold", indicatorGold);
        CheckWeeklyRewardIndicator("Set", indicatorSet);
        CheckOneTimeRewardIndicator("Fb", indicatorFb);
        CheckOneTimeRewardIndicator("Dis", indicatorDis);
    }

    private void CheckWeeklyRewardIndicator(string rewardKey, GameObject indicator)
    {
        string lastClaimKey = $"WeeklyReward_{rewardKey}";

        if (!PlayerPrefs.HasKey(lastClaimKey))
        {
            indicator.SetActive(true); // Chưa từng nhận, có thể nhận
        }
        else
        {
            DateTime lastClaim = DateTime.Parse(PlayerPrefs.GetString(lastClaimKey));
            bool canClaim = (DateTime.Now - lastClaim).TotalDays >= cooldownDays;
            indicator.SetActive(canClaim);
        }
    }

    private void CheckOneTimeRewardIndicator(string rewardKey, GameObject indicator)
    {
        string claimKey = $"OneTimeReward_{rewardKey}";
        bool claimed = PlayerPrefs.GetInt(claimKey, 0) == 1;
        indicator.SetActive(!claimed); // Hiện nếu chưa nhận
    }
    public void Next7Days()
    {
        string[] weeklyKeys = { "WeeklyReward_Gold", "WeeklyReward_Set" };

        foreach (string key in weeklyKeys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                DateTime lastClaim = DateTime.Parse(PlayerPrefs.GetString(key));
                DateTime fakeOldClaim = lastClaim.AddDays(-cooldownDays); // Giả lập đã nhận từ 7 ngày trước
                PlayerPrefs.SetString(key, fakeOldClaim.ToString());
            }
            else
            {
                // Nếu chưa có key, tạo key giả từ 7 ngày trước
                DateTime fakeOldClaim = DateTime.Now.AddDays(-cooldownDays);
                PlayerPrefs.SetString(key, fakeOldClaim.ToString());
            }
        }

        PlayerPrefs.Save();
        UpdateAllIndicators();
        ShowMessage(" Đã giả lập qua 7 ngày!");
    }

}
