using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth; // nếu bạn còn dùng UnlockMode ở map cuối

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveData { public List<GameObject> enemies; }

    [System.Serializable]
    public class MapData
    {
        public List<WaveData> waves;
        public GameObject teleportBox;          // Hiện khi clear xong map
        public Transform playerTargetPosition;  // Vị trí dịch chuyển sang map kế
        public int rewardGold;
    }

    [Header("Maps & Player")]
    public List<MapData> maps;
    public Transform player;
    public GameObject panelWin;
    public GameObject VFXWin;

    [Header("Rewards")]
    public int GoldWave = 0;
    public int GemWave = 0;

    [Header("Skill Unlock (Rain of Bullets)")]
    public RainOfBulletsSkill rainSkill;
    [Tooltip("0 = Map 1, 1 = Map 2, 2 = Map 3 ...")]
    public int unlockMapIndex = 1; // Map 2
    private bool rainUnlockedFired = false; // chặn gọi lặp trong 1 lần clear

    [Header("Unlock UI Banner (Skill)")]
    public TbUnlockSkill unlockBanner; // banner mở khoá Mưa Đạn

    // ===== Win Audio (CHỈ map cuối) =====
    [Header("Win Audio (last map only)")]
    public AudioClip winClip;                  // kéo clip win vào
    [Range(0f, 1f)] public float winVolume = 1f;
    public AudioSource mixerReference;         // giữ routing AudioMixer nếu có
    private GameObject winAudioGO;
    private bool winAudioHooked = false;
    private bool winSoundPlayed = false;

    private int currentMapIndex = 0;
    private int currentWaveIndex = 0;
    private bool coinAbsorbed = false;

    void Start()
    {
        // Tắt mọi teleport box khi vào scene để tránh banner Unlock Map bật sớm
        foreach (var m in maps)
            if (m.teleportBox) m.teleportBox.SetActive(false);

        // Nếu skill đã mở từ trước (theo PlayerPrefs của chính skill) thì bật component
        EnsureRainSkillIfUnlocked();
        SpawnWave(currentWaveIndex);
    }

    void OnDisable()
    {
        CleanupWinAudio(false);
    }

    void Update()
    {
        if (currentMapIndex >= maps.Count) return;
        var currentMap = maps[currentMapIndex];

        // Đã clear hết wave trong map hiện tại
        if (currentWaveIndex >= currentMap.waves.Count)
        {
            // Bật teleporter và show banner “Unlock Map” đúng lúc clear map (không phát âm ở đây)
            if (currentMap.teleportBox && !currentMap.teleportBox.activeSelf)
            {
                currentMap.teleportBox.SetActive(true);
                var tp = currentMap.teleportBox.GetComponent<TeleportBox>();
                if (tp != null) tp.ShowUnlockBannerSafely();
            }

            // Mở Mưa Đạn khi vừa clear Map 2 (chỉ 1 lần)
            if (!coinAbsorbed && currentMapIndex == unlockMapIndex)
            {
                HandleRainUnlockOnce();
            }

            if (!coinAbsorbed)
            {
                coinAbsorbed = true;
                Invoke(nameof(AttractCoinsToPlayer), 0.2f);

                // === CHỈ map cuối mới phát âm thanh win ===
                if (currentMapIndex == maps.Count - 1)
                {
                    GemWave = 15;
                    GoldWave += 50;

                    PlayWinSoundUntilSceneChange(); // <-- chỉ gọi ở map cuối

                    // Backend khác nếu bạn dùng
                    FindObjectOfType<FireBaseDataBaseManager>()?.UnlockMode(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
                    if (panelWin) panelWin.SetActive(true);
                    if (VFXWin) VFXWin.SetActive(true);
                }
            }
            return;
        }

        // Dọn list quái & sang wave kế
        var wave = currentMap.waves[currentWaveIndex];
        wave.enemies.RemoveAll(e => e == null);

        if (wave.enemies.Count == 0)
        {
            currentWaveIndex++;
            SpawnWave(currentWaveIndex);
        }
    }

    private void SpawnWave(int waveIndex)
    {
        if (currentMapIndex >= maps.Count) return;
        var currentMap = maps[currentMapIndex];
        if (waveIndex >= currentMap.waves.Count) return;

        foreach (var enemy in currentMap.waves[waveIndex].enemies)
            if (enemy != null) enemy.SetActive(true);
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
        foreach (var coin in FindObjectsOfType<CollectCoin>())
            coin.ActivateMagnet(player);
    }

    // ===== Skill helpers =====
    private void EnsureRainSkillIfUnlocked()
    {
        if (rainSkill != null && rainSkill.isUnlocked)
            rainSkill.enabled = true;
    }

    private void HandleRainUnlockOnce()
    {
        if (rainUnlockedFired) return;
        rainUnlockedFired = true;

        if (rainSkill == null) return;

        bool wasUnlocked = rainSkill.isUnlocked;

        if (!wasUnlocked)
        {
            // LẦN ĐẦU mở thật sự: unlock + enable + hiện banner
            rainSkill.Unlock();      // tự lưu PlayerPrefs trong skill
            rainSkill.enabled = true;

            if (unlockBanner != null) unlockBanner.showTb();
            else FindObjectOfType<TbUnlockSkill>()?.showTb();

            Debug.Log("[MapManager] RainOfBullets unlocked (FIRST TIME) -> show banner.");
        }
        else
        {
            // Đã mở từ trước: chỉ enable, không banner
            rainSkill.enabled = true;
            Debug.Log("[MapManager] RainOfBullets already unlocked -> NO banner.");
        }
    }

    // ===== Win Sound (ONLY last map) =====
    private void PlayWinSoundUntilSceneChange()
    {
        if (winSoundPlayed || winClip == null) return;
        winSoundPlayed = true;

        winAudioGO = new GameObject("WinSound_UntilSceneChange");
        var src = winAudioGO.AddComponent<AudioSource>();
        src.clip = winClip;
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f;
        src.volume = winVolume;

        if (mixerReference != null && mixerReference.outputAudioMixerGroup != null)
            src.outputAudioMixerGroup = mixerReference.outputAudioMixerGroup;

        DontDestroyOnLoad(winAudioGO);

        if (!winAudioHooked)
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged_StopWinAudio;
            winAudioHooked = true;
        }

        src.Play();
        StartCoroutine(DestroyAfterUnscaled(src.clip.length + 0.1f));
    }

    private System.Collections.IEnumerator DestroyAfterUnscaled(float seconds)
    {
        float t = 0f;
        while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        CleanupWinAudio(false);
    }

    private void OnActiveSceneChanged_StopWinAudio(Scene oldScene, Scene newScene)
    {
        CleanupWinAudio(true);
    }

    private void CleanupWinAudio(bool fromSceneChange)
    {
        if (winAudioHooked)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged_StopWinAudio;
            winAudioHooked = false;
        }

        if (winAudioGO != null)
        {
            var src = winAudioGO.GetComponent<AudioSource>();
            if (src != null && fromSceneChange && src.isPlaying) src.Stop();
            Destroy(winAudioGO);
            winAudioGO = null;
        }
    }
}
