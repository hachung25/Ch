using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveData
    {
        public List<GameObject> enemies; // Danh sách quái của mỗi wave
    }

    [System.Serializable]
    public class MapData
    {
        public List<WaveData> waves;         // Các wave trong map
        public GameObject teleportBox;       // Xuất hiện khi hết map
        public Transform playerTargetPosition; 
        public int rewardGold;               // Thưởng vàng khi xong map
    }

    public List<MapData> maps;             // Danh sách các map (gồm nhiều wave)
    public Transform player;               // Gán player từ Inspector
    public GameObject panelWin;
    public GameObject VFXWin;

    public int GoldWave = 0;
    public int GemWave = 0;

    private int currentMapIndex = 0;
    private int currentWaveIndex = 0;
    private bool coinAbsorbed = false;
 

    void Start()
    {
        SpawnWave(currentWaveIndex); // Bắt đầu wave đầu tiên của map đầu tiên
    }

    void Update()
    {
        if (currentMapIndex >= maps.Count) return;

        MapData currentMap = maps[currentMapIndex];

        // Nếu đã hoàn thành hết các wave trong map
        if (currentWaveIndex >= currentMap.waves.Count)
        {
            if (!currentMap.teleportBox.activeSelf)
                currentMap.teleportBox.SetActive(true);
            
          
            
            if (!coinAbsorbed)
            {
                coinAbsorbed = true;
                Invoke(nameof(AttractCoinsToPlayer), 0.2f);

                if (currentMapIndex == 4) // Map cuối cùng
                {
                    GemWave = 15;
                    GoldWave += 50;
                    FindObjectOfType<FireBaseDataBaseManager>()?.UnlockMode(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
                    panelWin.SetActive(true);
                    VFXWin.SetActive(true);
                }
            }

            return;
        }

        WaveData currentWave = currentMap.waves[currentWaveIndex];

        // Xoá quái đã bị tiêu diệt
        currentWave.enemies.RemoveAll(enemy => enemy == null);

        // Nếu wave hiện tại đã diệt hết quái
        if (currentWave.enemies.Count == 0)
        {
            currentWaveIndex++;
            SpawnWave(currentWaveIndex);
        }
    }
    

    private void SpawnWave(int waveIndex)
    {
        if (currentMapIndex >= maps.Count) return;

        MapData currentMap = maps[currentMapIndex];
        if (waveIndex >= currentMap.waves.Count) return;

        WaveData wave = currentMap.waves[waveIndex];
        foreach (var enemy in wave.enemies)
        {
            if (enemy != null)
                enemy.SetActive(true); // Bật quái lên nếu đã tắt sẵn
        }
    }

    public void MoveToNextMap()
    {
        if (currentMapIndex < maps.Count)
        {
            GoldWave += maps[currentMapIndex].rewardGold;

            player.position = maps[currentMapIndex].playerTargetPosition.position;

            maps[currentMapIndex].teleportBox.SetActive(false);
       
            currentMapIndex++;
            currentWaveIndex = 0;
            coinAbsorbed = false;

            SpawnWave(currentWaveIndex);
        }
    }

    private void AttractCoinsToPlayer()
    {
        var coins = FindObjectsOfType<CollectCoin>();
        foreach (var coin in coins)
        {
            coin.ActivateMagnet(player);
        }
    }
}
