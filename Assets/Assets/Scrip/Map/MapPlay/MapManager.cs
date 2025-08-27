using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // <-- THÊM
using Firebase.Auth;

public class MapManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveData
    {
        public List<GameObject> enemies;
    }

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
    public int unlockMapIndex = 1;
    private bool rainUnlockedFired = false;

    // === ÂM THANH THẮNG CUỘC ===
    [Header("Win Audio")]
    public AudioClip winClip;                   // KÉO clip chiến thắng vào đây
    [Range(0f, 1f)] public float winVolume = 1f;
    public AudioSource mixerReference;          // (tuỳ chọn) kéo AudioSource để giữ OutputAudioMixerGroup
    private GameObject winAudioGO;              // GO phát âm thanh sống tới khi đổi scene
    private bool winAudioHooked = false;        // đã gắn callback scene change chưa
    private bool winSoundPlayed = false;        // đảm bảo chỉ phát 1 lần

    private int currentMapIndex = 0;
    private int currentWaveIndex = 0;
    private bool coinAbsorbed = false;

    void Start()
    {
        SpawnWave(currentWaveIndex);
    }

    void OnDisable()
    {
        // dọn dẹp listener nếu object bị disable trước khi đổi scene
        CleanupWinAudio(false);
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

                // Map cuối cùng: phát nhạc win tới khi chuyển scene
                if (currentMapIndex == maps.Count - 1)
                {
                    GemWave = 15;
                    GoldWave += 50;

                    // Phát nhạc chiến thắng (tồn tại qua scene trừ khi scene đổi)
                    PlayWinSoundUntilSceneChange();

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
                enemy.SetActive(true);
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
        if (rainUnlockedFired) return;
        if (currentMapIndex != unlockMapIndex) return;

        rainUnlockedFired = true;

        if (rainSkill != null)
        {
            rainSkill.Unlock();
            rainSkill.enabled = true;
        }

        Debug.Log("Unlocked RainOfBullets at map index: " + currentMapIndex);
        // FindObjectOfType<FireBaseDataBaseManager>()?.SetRainSkillUnlocked(FirebaseAuth.DefaultInstance.CurrentUser.UserId, true);
    }

    // ===================== WIN SOUND =====================
    private void PlayWinSoundUntilSceneChange()
    {
        if (winSoundPlayed || winClip == null) return;
        winSoundPlayed = true;

        // tạo GO và AudioSource độc lập
        winAudioGO = new GameObject("WinSound_UntilSceneChange");
        var src = winAudioGO.AddComponent<AudioSource>();
        src.clip = winClip;
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D
        src.volume = winVolume;

        // giữ routing mixer nếu có
        if (mixerReference != null && mixerReference.outputAudioMixerGroup != null)
            src.outputAudioMixerGroup = mixerReference.outputAudioMixerGroup;

        DontDestroyOnLoad(winAudioGO);

        // đăng ký callback đổi scene (1 lần)
        if (!winAudioHooked)
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged_StopWinAudio;
            winAudioHooked = true;
        }

        src.Play();

        // nếu không đổi scene thì GO tự hủy sau khi phát xong
        StartCoroutine(DestroyAfterUnscaled(src.clip.length + 0.1f));
    }

    private System.Collections.IEnumerator DestroyAfterUnscaled(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        // phát xong mà chưa đổi scene -> cleanup
        CleanupWinAudio(false);
    }

    private void OnActiveSceneChanged_StopWinAudio(Scene oldScene, Scene newScene)
    {
        // vừa đổi scene -> dừng và hủy ngay
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
            if (src != null && fromSceneChange && src.isPlaying)
                src.Stop();

            Destroy(winAudioGO);
            winAudioGO = null;
        }
    }
}
