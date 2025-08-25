using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using System.Text.RegularExpressions;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Text cho 6 người đứng đầu")]
    public TMP_Text[] topNameTexts;     // Text tên: 0 -> 5
    public TMP_Text[] topTicketTexts;   // Text ticket: 0 -> 5

    [Header("Text hiển thị người chơi hiện tại")]
    public TMP_Text currentNameText;
    public TMP_Text currentTicketText;
    public TMP_Text currentRankText;

    public GameObject rank1, rank2, rank3, rank4, rank5;
    public GameObject UI1, UI2 ,bt1, bt2;
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
    

    public void CheckWinRank()
    {
        
        if (currentRankText != null)
        {
            string rankString = currentRankText.text; 
            int rank = -1; 
            
            Match match = Regex.Match(rankString, @"\d+");
            if (match.Success)
            {
                rank = int.Parse(match.Value);
            }
            if (rank == 1)
            {
              rank1.SetActive(true);
            }
            if (rank == 2)
            {
                rank2.SetActive(true);
            }
            if (rank == 3)
            {
               rank3.SetActive(true);
            }
            if (rank >= 4 && rank <= 10)
            {
                rank4.SetActive(true);
            }
            if (rank > 10)
            {
                rank5.SetActive(true);
            }
            
        }
        UI1.SetActive(false);
        UI2.SetActive(false);
        bt1.SetActive(false);
        bt2.SetActive(false);
    }


}