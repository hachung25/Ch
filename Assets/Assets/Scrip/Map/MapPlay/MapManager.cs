using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class MapData
    {
        public List<GameObject> enemies;       // Các quái trong map
        public GameObject teleportBox;         // Box xuất hiện khi hết quái
        public Transform playerTargetPosition; // Vị trí dịch chuyển
    }

    public List<MapData> maps;     // Gồm 5 map
    public Transform player;       // Gán Player vào đây
    private int currentMapIndex = 0;

    void Update()
    {
        if (currentMapIndex >= maps.Count) return;

        MapData currentMap = maps[currentMapIndex];

        // Kiểm tra nếu tất cả quái đã bị tiêu diệt
        currentMap.enemies.RemoveAll(enemy => enemy == null); // Xóa các quái đã bị diệt
        if (currentMap.enemies.Count == 0 && !currentMap.teleportBox.activeSelf)
        {
            currentMap.teleportBox.SetActive(true);
         
        }
        currentMap.enemies.RemoveAll(enemy => enemy == null);

        if (currentMap.enemies.Count == 0)
        {
            if (!currentMap.teleportBox.activeSelf)
            {
                currentMap.teleportBox.SetActive(true);
            }

            if (!coinAbsorbed)
            {
                coinAbsorbed = true;
                AttractCoinsToPlayer();
            }
        }
    }

    public void MoveToNextMap()
    {
        if (currentMapIndex < maps.Count)
        {
            player.position = maps[currentMapIndex].playerTargetPosition.position;
            maps[currentMapIndex].teleportBox.SetActive(false); // Ẩn box sau khi chuyển
            currentMapIndex++;
            coinAbsorbed = false;
        }
    }
    private bool coinAbsorbed = false;
    private void AttractCoinsToPlayer()
    {
        var coins = FindObjectsOfType<CollectCoin>();
        foreach (var coin in coins)
        {
            coin.ActivateMagnet(); // Coin tự động bay về Player
        }
    }
    
}
