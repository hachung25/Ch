using System;
using TMPro;
using UnityEngine;

public class CountdownTimer2th9 : MonoBehaviour
{
    public TMP_Text countdownText; // Gán Text để hiển thị
    public string targetTimeString = "2025-09-07 20:00:00"; // Thời gian đích, định dạng yyyy-MM-dd HH:mm:ss

    private DateTime targetTime;
    private bool isCounting = false;

    void Start()
    {
        // Chuyển chuỗi thành DateTime
        if (DateTime.TryParse(targetTimeString, out targetTime))
        {
            isCounting = true;
        }
        else
        {
            countdownText.text = "⛔ Sai định dạng thời gian!";
        }
    }

    void Update()
    {
        if (!isCounting) return;

        TimeSpan remaining = targetTime - DateTime.Now;

        if (remaining.TotalSeconds <= 0)
        {
            countdownText.text = "Đã hết thời gian!";
            isCounting = false;
            return;
        }

        // Nếu còn trên 1 ngày
        if (remaining.TotalDays >= 1)
        {
            int days = Mathf.FloorToInt((float)remaining.TotalDays);
            countdownText.text = $"Còn {days} ngày";
        }
        else
        {
            // Còn dưới 1 ngày → hiển thị giờ:phút:giây
            string timeFormatted = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            countdownText.text = $"Còn {timeFormatted}";
        }
    }
}
