using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private int currentHealth;
    public int MaxHealth { get; private set; }

    public bool isDead = false;
    public bool isInvincible = false;

    private Animator animator;
    public PlayerMovement2D playerMovement;
    public GameObject LoseGame;
    public GameObject hitVFXPrefab;
    public GameObject DeadVFXPrefab;
    public EnemyFlyGhost Ghost;

    public AudioClip hitSound;       // âm thanh bạn chọn
    private AudioSource audioSource; // để phát âm thanh
    public AudioClip OverSound;

    private GameObject overAudioGO;
    private bool overAudioHooked = false;
    private void OnEnable()
    {
        IndexPlayerPlayGame.OnStatsLoaded += SetHealth;
    }

    private void OnDisable()
    {
        IndexPlayerPlayGame.OnStatsLoaded -= SetHealth;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        // Lấy AudioSource sẵn có hoặc tự thêm
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void SetHealth(int health, int damage)
    {
        if (isDead) return;

        MaxHealth = health;
        currentHealth = MaxHealth;

        Debug.Log("Máu đã được gán từ Firebase: " + MaxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        Debug.Log($"Player nhận {damage} sát thương. Máu còn: {currentHealth}");
        if (hitVFXPrefab != null)
        {
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
        }
        if (animator != null)
            animator.SetTrigger("Hit");

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player đã chết!");

        if (playerMovement != null)
            playerMovement.Dead();

        GetComponent<Rigidbody2D>().simulated = false;
        StartCoroutine(FlashWhileInvincible());

        if (DeadVFXPrefab != null)
            Instantiate(DeadVFXPrefab, transform.position, Quaternion.identity);

        // PHÁT ÂM THANH TÁCH RỜI & TẮT KHI CHUYỂN SCENE
        PlayOverSoundUntilSceneChange(OverSound, audioSource);

        Destroy(gameObject, 0.7f);

        if (LoseGame != null) LoseGame.SetActive(true);
        if (Ghost != null) Ghost.FreezeEnemy();
    }

    // === THÊM: phát âm thanh tách rời, sống qua scene cho đến khi scene đổi
    private void PlayOverSoundUntilSceneChange(AudioClip clip, AudioSource reference = null, float volume = 1f)
    {
        if (clip == null) return;

        // Tạo GO + AudioSource độc lập
        overAudioGO = new GameObject("OverSound_PlayerDeath");
        var src = overAudioGO.AddComponent<AudioSource>();
        src.clip = clip;
        src.playOnAwake = false;
        src.loop = false;           // không lặp, chỉ phát 1 lần
        src.spatialBlend = 0f;      // 2D
        src.volume = volume;

        // Giữ routing mixer nếu bạn có dùng Mixer
        if (reference != null && reference.outputAudioMixerGroup != null)
            src.outputAudioMixerGroup = reference.outputAudioMixerGroup;

        DontDestroyOnLoad(overAudioGO);

        // Đăng ký callback 1 lần
        if (!overAudioHooked)
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged_StopOverAudio;
            overAudioHooked = true;
        }

        // Phát
        src.Play();

        // Tự cleanup nếu clip phát xong mà không đổi scene
        overAudioGO.AddComponent<AutoDestroyAfter>()
                   .Init(src.clip.length + 0.1f, () =>
                   {
                       // nếu GO này vẫn còn và chưa bị scene change hủy
                       CleanupOverAudio(false);
                   });
    }
    private void OnActiveSceneChanged_StopOverAudio(Scene oldScene, Scene newScene)
    {
        CleanupOverAudio(true);
    }
    private void CleanupOverAudio(bool fromSceneChange)
    {
        // hủy đăng ký callback
        if (overAudioHooked)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged_StopOverAudio;
            overAudioHooked = false;
        }

        if (overAudioGO != null)
        {
            var src = overAudioGO.GetComponent<AudioSource>();
            if (src != null)
            {
                if (fromSceneChange && src.isPlaying) src.Stop();
            }
            Destroy(overAudioGO);
            overAudioGO = null;
        }
    }
    public void Heal(int amount)
    {
        if (isDead) return;

        int before = currentHealth;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

    }


    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ResetHealth()
    {
        isDead = false;
        isInvincible = false;

        MaxHealth = IndexPlayerPlayGame.PlayerHealthValue;
        currentHealth = MaxHealth;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private System.Collections.IEnumerator FlashWhileInvincible()
    {
        float duration = 0.7f;
        float timer = 0f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        while (timer < duration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        sr.enabled = true;
        isInvincible = false;
    }
}
