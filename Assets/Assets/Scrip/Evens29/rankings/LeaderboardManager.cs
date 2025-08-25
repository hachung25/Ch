using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public FirebaseRankingLoader loader;      // Gán trong Inspector
    public LeaderboardUI leaderboardUI;       // Gán trong Inspector

    void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        loader.LoadTopRanking(dataList =>
        {
            leaderboardUI.ShowTop6AndCurrentUser(dataList);
        });
    }
}
