using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using System;

public class AdmobRewarded : MonoBehaviour
{
    private RewardedAd rewardedAd;
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test

    void Start()
    {
        MobileAds.Initialize(initStatus => {
            LoadRewardedAd();
        });
    }

    public void LoadRewardedAd()
    {
        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load: " + error);
                return;
            }

            rewardedAd = ad;
            Debug.Log("Rewarded ad loaded.");

            rewardedAd.OnAdFullScreenContentClosed += () => {
                Debug.Log("Ad closed.");
                LoadRewardedAd(); // Load lại
            };

            rewardedAd.OnAdFullScreenContentFailed += (err) => {
                Debug.LogError("Ad failed to show: " + err);
            };

            rewardedAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"Ad revenue: {adValue.Value}");
            };
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) => {
                Debug.Log("Người dùng đã xem xong, thưởng: " + reward.Amount);
                // TODO: Thưởng cho người chơi
            });
        }
        else
        {
            Debug.Log("Ad chưa sẵn sàng.");
        }
    }
}
