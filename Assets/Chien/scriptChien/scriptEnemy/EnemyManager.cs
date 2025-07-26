using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int enemyAliveCount = 0;

    [Header("Tham chiếu tới Player")]
    public Transform player;

    private bool coinsAbsorbed = false; // để tránh gọi nhiều lần

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (!coinsAbsorbed && AllEnemiesDead())
        {
            coinsAbsorbed = true; // tránh gọi liên tục
            AttractCoinsToPlayer();
        }
    }

    public void RegisterEnemy()
    {
        enemyAliveCount++;
        coinsAbsorbed = false; // reset lại nếu có enemy mới
    }

    public void UnregisterEnemy()
    {
        enemyAliveCount--;
        if (enemyAliveCount < 0) enemyAliveCount = 0;
        Debug.Log("Enemy còn lại: " + enemyAliveCount);
    }

    public bool AllEnemiesDead()
    {
        return enemyAliveCount <= 0;
    }

    private void AttractCoinsToPlayer()
    {
        var coins = FindObjectsOfType<CollectCoin>();
        foreach (var coin in coins)
        {
            coin.ActivateMagnet(); // KHÔNG truyền player
        }
    }
}
