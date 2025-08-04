using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class MapData
    {
        public List<GameObject> enemies;       // Các quái trong map
        public GameObject teleportBox;         // Box xuất hiện khi hết quái
        public Transform playerTargetPosition; 
        public int rewardGold;       
    }

    public List<MapData> maps;     // Gồm 5 map
    public Transform player;       // Gán Player vào đây
    private int currentMapIndex = 0;

    public int GoldWave = 0;
    public int GemWave = 0;
    public GameObject panelWin;

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
                Invoke(nameof(AttractCoinsToPlayer), 0.2f); // Delay nhẹ để đợi coin được tạo
                
                if (currentMapIndex == 4)
                {
                    GemWave = 15;
                    GoldWave += 50;
                    FindObjectOfType<FireBaseDataBaseManager>()?.UnlockMode(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
                  panelWin.SetActive(true);
                }
            }
        }
    }
 

 
    public void MoveToNextMap()
    {
        if (currentMapIndex < maps.Count)
        {
            // 🌟 Nhận vàng từ map hiện tại
            GoldWave += maps[currentMapIndex].rewardGold;
            
            Debug.Log("Gold wave: " + GoldWave);
            player.position = maps[currentMapIndex].playerTargetPosition.position;
            maps[currentMapIndex].teleportBox.SetActive(false);
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
            coin.ActivateMagnet(player); 
        }
    }

}
