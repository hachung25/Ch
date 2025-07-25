using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int enemyAliveCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterEnemy()
    {
        enemyAliveCount++;
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
}
