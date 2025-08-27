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

    [Header("Maps & Player")]
    public List<MapData> maps;             // Danh sách các map
    public Transform player;               // Gán player từ Inspector
    public GameObject panelWin;
    public GameObject VFXWin;

    [Header("Rewards")]
    public int GoldWave = 0;
    public int GemWave = 0;

    [Header("Skill Unlock")]
    public RainOfBulletsSkill rainSkill;      // Kéo component trên Player vào đây
    [Tooltip("0 = Map 1, 1 = Map 2, 2 = Map 3 ...")]
    public int unlockMapIndex = 1;            // Mặc định mở ở Map 2 (đếm người)
    private bool rainUnlockedFired = false;   // đảm bảo chỉ mở 1 lần trong session

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
            if (currentMap.teleportBox && !currentMap.teleportBox.activeSelf)
                currentMap.teleportBox.SetActive(true);

            // Thử mở khóa skill khi clear xong map cần mở
            TryUnlockRainSkill();

            if (!coinAbsorbed)
            {
                coinAbsorbed = true;
                Invoke(nameof(AttractCoinsToPlayer), 0.2f);

                // Ví dụ: Map cuối cùng là index 4
                if (currentMapIndex == 4)
                {
                    GemWave = 15;
                    GoldWave += 50;
                    FindObjectOfType<FireBaseDataBaseManager>()?.UnlockMode(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
                    if (panelWin) panelWin.SetActive(true);
                    if (VFXWin) VFXWin.SetActive(true);
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

            if (player && maps[currentMapIndex].playerTargetPosition)
                player.position = maps[currentMapIndex].playerTargetPosition.position;

            if (maps[currentMapIndex].teleportBox)
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

    // ==== Skill unlock hook ====
    private void TryUnlockRainSkill()
    {
        if (rainUnlockedFired) return;                 // chỉ chạy 1 lần mỗi scene
        if (currentMapIndex != unlockMapIndex) return; // chỉ khi vừa clear xong map cần mở

        rainUnlockedFired = true;

        if (rainSkill != null)
        {
            // Gọi trực tiếp hàm Unlock của skill (tự lưu PlayerPrefs bên trong)
            rainSkill.Unlock();
            // Bảo đảm component đang bật (nếu bạn có toggle ở nơi khác)
            rainSkill.enabled = true;
        }

        Debug.Log("Unlocked RainOfBullets at map index: " + currentMapIndex);

        // Nếu muốn lưu backend theo user, thêm tại đây (tuỳ hệ thống của bạn):
        // FindObjectOfType<FireBaseDataBaseManager>()?.SetRainSkillUnlocked(FirebaseAuth.DefaultInstance.CurrentUser.UserId, true);
    }
}
