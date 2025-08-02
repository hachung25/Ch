using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Text cho 6 người đứng đầu")]
    public TMP_Text[] topNameTexts;     // Text tên: 0 -> 5
    public TMP_Text[] topTicketTexts;   // Text ticket: 0 -> 5

    [Header("Text hiển thị người chơi hiện tại")]
    public TMP_Text currentNameText;
    public TMP_Text currentTicketText;
    public TMP_Text currentRankText;

    public void ShowTop6AndCurrentUser(List<RankingData> fullData)
    {
        // 👉 Sắp xếp giảm dần theo số ticket
        fullData.Sort((a, b) => b.Ticket.CompareTo(a.Ticket));

        string currentUserId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        string yourName = "Bạn";
        int yourRank = -1;
        int yourTicket = 0;

        for (int i = 0; i < fullData.Count; i++)
        {
            if (fullData[i].UserId == currentUserId)
            {
                yourRank = i + 1;
                yourName = fullData[i].Name;
                yourTicket = fullData[i].Ticket;
                break;
            }
        }

        for (int i = 0; i < 6; i++)
        {
            if (i < fullData.Count)
            {
                var entry = fullData[i];
                if (i < topNameTexts.Length) topNameTexts[i].text = entry.Name;
                if (i < topTicketTexts.Length) topTicketTexts[i].text = $"{entry.Ticket} Tickets";
            }
            else
            {
                if (i < topNameTexts.Length) topNameTexts[i].text = "---";
                if (i < topTicketTexts.Length) topTicketTexts[i].text = "---";
            }
        }

        if (currentNameText != null) currentNameText.text = yourName;
        if (currentTicketText != null) currentTicketText.text = $"{yourTicket} Tickets";
        if (currentRankText != null)
        {
            currentRankText.text = (yourRank > 0)
                ? $"Hạng: {yourRank}"
                : "Bạn chưa có trong bảng xếp hạng";
        }
    }
}
