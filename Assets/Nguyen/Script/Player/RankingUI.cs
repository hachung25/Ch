using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class RankingUI : MonoBehaviour
{
    public Transform content;
    public GameObject rowPrefab;
    public Sprite[] medalIcons; // 0 = Gold, 1 = Silver, 2 = Bronze

    private void OnEnable()
    {
        PlayerStats.OnAnyStatsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        PlayerStats.OnAnyStatsChanged -= Refresh;
    }

    private void Refresh()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        List<PlayerStats> players = FindObjectsOfType<PlayerStats>()
            .OrderByDescending(p => p.Kills)
            .ToList();

        for (int rank = 0; rank < players.Count; rank++)
        {
            var row = Instantiate(rowPrefab, content);
            PlayerStats stats = players[rank];

            // Nickname
            string nick = "---";
            var nickSync = stats.GetComponent<NicknameSync>();
            if (nickSync != null && !string.IsNullOrEmpty(nickSync.Nickname.ToString()))
                nick = nickSync.Nickname.ToString();

            row.transform.Find("Text_NickName").GetComponent<TMP_Text>().text = nick;
            row.transform.Find("Text_Score").GetComponent<TMP_Text>().text = stats.Kills.ToString();

            // Medal vs Rank Text
            var medalImg = row.transform.Find("Icon_Medal")?.GetComponent<Image>();
            var rankText = row.transform.Find("Text_Rank")?.GetComponent<TMP_Text>();

            if (rank < 3 && medalIcons != null && rank < medalIcons.Length)
            {
                // Top 3 => dùng medal
                if (medalImg != null)
                {
                    medalImg.enabled = true;
                    medalImg.sprite = medalIcons[rank];
                }
                if (rankText != null) rankText.gameObject.SetActive(false);
            }
            else
            {
                // Rank >= 4 => hiện số
                if (medalImg != null) medalImg.enabled = false;
                if (rankText != null)
                {
                    rankText.gameObject.SetActive(true);
                    rankText.text = (rank + 1).ToString();
                }
            }
        }
    }
}
