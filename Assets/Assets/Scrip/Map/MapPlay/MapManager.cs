using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth; // vẫn giữ nếu bạn dùng UnlockMode ở map cuối

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveData { public List<GameObject> enemies; }

    [System.Serializable]
    public class MapData
    {
        public List<WaveData> waves;
        public GameObject teleportBox;
        public Transform playerTargetPosition;
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

    [Header("Skill Unlock")]
    public RainOfBulletsSkill rainSkill;
    [Tooltip("0 = Map 1, 1 = Map 2, 2 = Map 3 ...")]
    public int unlockMapIndex = 1;     // Map 2
    private bool rainUnlockedFired = false; // chặn double-call trong phiên

    // ===== Persist (local only)
    private const string RAIN_SKILL_KEY = "rain_skill_unlocked";
    private const string RAIN_BANNER_KEY = "rain_skill_banner_shown";
    private bool rainUnlockedPersistent = false;
    private bool rainBannerShownPersistent = false;

    [Header("Unlock UI Banner")]
    public TbUnlockSkill unlockBanner; // kéo object UI có TbUnlockSkill

    // === Âm thanh thắng cuộc ===
    [Header("Win Audio")]
    public AudioClip winClip;
    [Range(0f, 1f)] public float winVolume = 1f;
    public AudioSource mixerReference;
    private GameObject winAudioGO;
    private bool winAudioHooked = false;
    private bool winSoundPlayed = false;

    private int currentMapIndex = 0;
    private int currentWaveIndex = 0;
    private bool coinAbsorbed = false;

    // ===================== LIFECYCLE =====================
    void Start()
    {
        LoadRainFlags();
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

        if (currentWaveIndex >= currentMap.waves.Count)
        {
            if (currentMap.teleportBox && !currentMap.teleportBox.activeSelf)
                currentMap.teleportBox.SetActive(true);

            // !!! Chỉ chạy 1 lần khi vừa clear map
            if (!coinAbsorbed)
            {
                coinAbsorbed = true;

                // Mở khoá skill tại map 2 (one-shot)
                if (currentMapIndex == unlockMapIndex)
                    HandleRainUnlockOnce();

                // Hút coin
                Invoke(nameof(AttractCoinsToPlayer), 0.2f);

                // Map cuối: win sound + UI
                if (currentMapIndex == maps.Count - 1)
                {
                    GemWave = 15;
                    GoldWave += 50;

                    PlayWinSoundUntilSceneChange();

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

    // ===================== RAIN PERSIST =====================
    private void LoadRainFlags()
    {
        rainUnlockedPersistent = PlayerPrefs.GetInt(RAIN_SKILL_KEY, 0) == 1;
        rainBannerShownPersistent = PlayerPrefs.GetInt(RAIN_BANNER_KEY, 0) == 1;
    }

    private void SaveRainUnlocked()
    {
        PlayerPrefs.SetInt(RAIN_SKILL_KEY, 1);
        PlayerPrefs.Save();
        rainUnlockedPersistent = true;
    }

    private void MarkBannerShown()
    {
        PlayerPrefs.SetInt(RAIN_BANNER_KEY, 1);
        PlayerPrefs.Save();
        rainBannerShownPersistent = true;
    }

    private void EnsureRainSkillIfUnlocked()
    {
        if (rainUnlockedPersistent && rainSkill != null)
        {
            rainSkill.enabled = true;
            // nếu skill cần áp trạng thái, gọi thêm API của bạn tại đây
            // ví dụ: rainSkill.ApplyUnlockedState();
        }
    }

    /// <summary>
    /// Chỉ chạy 1 lần ngay khoảnh khắc clear map 2.
    /// Nếu đã mở vĩnh viễn -> chỉ bật skill, KHÔNG hiện banner.
    /// Nếu chưa mở -> mở + lưu + hiện banner (chỉ lần đầu).
    /// </summary>
    private void HandleRainUnlockOnce()
    {
        // nếu đã mở từ trước -> đảm bảo bật skill, không hiện banner
        if (rainUnlockedPersistent)
        {
            EnsureRainSkillIfUnlocked();
            return;
        }

        // chặn double-call trong phiên (phòng trường hợp call lại do logic khác)
        if (rainUnlockedFired) return;
        rainUnlockedFired = true;

        // mở lần đầu
        if (rainSkill != null)
        {
            rainSkill.Unlock();      // nếu bạn có logic nội bộ
            rainSkill.enabled = true;
        }

        SaveRainUnlocked();

        // Hiện banner lần đầu (set flag trước để không bị lặp trong cùng frame)
        if (!rainBannerShownPersistent)
        {
            MarkBannerShown();
            if (unlockBanner != null) unlockBanner.showTb();
            else FindObjectOfType<TbUnlockSkill>()?.showTb();
        }

        Debug.Log("[MapManager] RainOfBullets unlocked (first time).");
    }

    // ===================== WIN SOUND =====================
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
