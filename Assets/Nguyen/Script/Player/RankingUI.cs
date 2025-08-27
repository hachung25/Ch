using UnityEngine;
using TMPro;
using System.Linq;

public class RankingUI : MonoBehaviour
{
    public Transform content;   // Gắn Content của ScrollView
    public GameObject rowPrefab; // Prefab 1 dòng (Text_NickName + Text_Score)

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

        var players = FindObjectsOfType<PlayerStats>()
            .OrderByDescending(p => p.Kills)
            .ToList();

        Debug.Log($"[RankingUI] Có {players.Count} players");

        foreach (var stats in players)
        {
            var row = Instantiate(rowPrefab, content);

            // 🔹 Lấy Nickname từ NicknameSync
            string nick = "Player?";
            var nickSync = stats.GetComponent<NicknameSync>();
            if (nickSync != null && !string.IsNullOrEmpty(nickSync.Nickname.ToString()))
                nick = nickSync.Nickname.ToString();

            row.transform.Find("Text_NickName").GetComponent<TMP_Text>().text = nick;
            row.transform.Find("Text_Score").GetComponent<TMP_Text>().text = stats.Kills.ToString();
        }
    }
}
