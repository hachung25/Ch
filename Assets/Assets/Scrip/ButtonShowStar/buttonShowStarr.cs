using UnityEngine;
using System;

public class buttonShowStarr : MonoBehaviour
{
    private const string LAST_CLAIM_DATE_KEY = "LastDailyClaimDate";

    void Start()
    {
        // Nếu đã nhận rồi thì ẩn GameObject
        if (!CanClaimToday())
        {
            gameObject.SetActive(false);
        }
    }

    public void Clemd()
    {
        if (CanClaimToday())
        {
            CardSpingManeger.AddCardSping(1);
            PlayerPrefs.SetString(LAST_CLAIM_DATE_KEY, DateTime.Now.ToString("yyyyMMdd")); // Lưu ngày nhận
            PlayerPrefs.Save();

            // Ẩn nút sau khi nhận
            gameObject.SetActive(false);
        }
    }

    private bool CanClaimToday()
    {
        string lastClaim = PlayerPrefs.GetString(LAST_CLAIM_DATE_KEY, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        return lastClaim != today;
    }
}