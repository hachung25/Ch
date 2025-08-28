using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class RankingUI : MonoBehaviour
{
    public Transform content;
    public GameObject rowPrefab;
    public Sprite[] medalIcons; // 0=Gold,1=Silver,2=Bronze

    [Header("Reward UI")]
    public TMP_Text rewardText; // Gắn Text dưới cùng

    // Quy định phần thưởng theo rank
    private readonly (int gold, int gem)[] rewards =
    {
        (50, 20), // Top 1
        (40, 15),  // Top 2
        (30, 10),  // Top 3
        (20, 5),   // Top 4+
    };

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

        int localRank = -1;

        for (int rank = 0; rank < players.Count; rank++)
        {
            var row = Instantiate(rowPrefab, content);
            var stats = players[rank];

            // Nickname
            string nick = stats.GetComponent<NicknameSync>()?.Nickname.ToString() ?? "---";
            row.transform.Find("Text_NickName").GetComponent<TMP_Text>().text = nick;
            row.transform.Find("Text_Score").GetComponent<TMP_Text>().text = stats.Kills.ToString();

            // Medal / Rank
            var medalImg = row.transform.Find("Icon_Medal")?.GetComponent<Image>();
            var rankText = row.transform.Find("Text_Rank")?.GetComponent<TMP_Text>();

            if (rank < 3)
            {
                if (medalImg != null)
                {
                    medalImg.enabled = true;
                    medalImg.sprite = medalIcons[rank];
                }
                if (rankText != null) rankText.gameObject.SetActive(false);
            }
            else
            {
                if (medalImg != null) medalImg.enabled = false;
                if (rankText != null)
                {
                    rankText.gameObject.SetActive(true);
                    rankText.text = (rank + 1).ToString();
                }
            }

            // Nếu đây là local player → lưu rank
            if (stats.Object.HasInputAuthority)
                localRank = rank + 1;
        }

        // Hiển thị thưởng cho local player
        if (rewardText != null && localRank > 0)
        {
            int gold = 0, gem = 0;

            if (localRank == 1) { gold = rewards[0].gold; gem = rewards[0].gem; }
            else if (localRank == 2) { gold = rewards[1].gold; gem = rewards[1].gem; }
            else if (localRank == 3) { gold = rewards[2].gold; gem = rewards[2].gem; }
            else { gold = rewards[3].gold; gem = rewards[3].gem; }

            rewardText.text = $"Bạn TOP {localRank}: +{gold} Gold +{gem}";
        }
    }
}
