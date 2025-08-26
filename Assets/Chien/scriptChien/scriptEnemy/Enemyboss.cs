using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class Enemyboss : EnemyGroundBase, IDamageable
{
    [Header("Thanh máu riêng")]
    public Slider overrideHealthSlider_boss;

    [Header("Tùy chỉnh chỉ số")]
    public int attackDamage = 8;
    public GameObject CoinPrefab;
    public GameObject DeadVFX;
    public AudioClip Sound_Destroy;
    private AudioSource Sound_Play;
    public bool isInvincible = false;
    protected override void Start()
    {
        base.Start();
        isFacingRightByDefault = true;
        currentHealth = maxHealth;

        if (overrideHealthSlider_boss != null)
        {
            healthSlider = overrideHealthSlider_boss;
            healthSlider.maxValue = maxHealth;
        }

        UpdateHealthBar();
        if(Sound_Play == null)
        {
            Sound_Play = gameObject.AddComponent<AudioSource>();
        }
        
    }

    // ===== NHẬN SÁT THƯƠNG TỪ PLAYER =====
    public override void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        Debug.Log($"{gameObject.name} nhận {damage} dame. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    //protected override void Die()
    //{
    //    if (isDead) return;

    //    isDead = true;

    //    if (healthSlider != null)
    //        healthSlider.gameObject.SetActive(false);

    //    if (animator != null)
    //        animator.SetTrigger("Die");
    //    if (CoinPrefab != null)
    //    {
    //        Instantiate(CoinPrefab, transform.position, Quaternion.identity);
    //    }
    //    GetComponent<Rigidbody2D>().simulated = false;
    //    Debug.Log($"{gameObject.name} đã chết!");
    //    if (DeadVFX != null)
    //    {
    //        Instantiate(DeadVFX, transform.position, Quaternion.identity);
    //    }

    //    Destroy(gameObject);
    //    if(Sound_Destroy != null && Sound_Play != null) 
    //    {
    //        Sound_Play.PlayOneShot(Sound_Destroy);
    //    }
    //}
    protected override void Die()
    {
        if (isDead) return;

        isDead = true;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        if (animator != null)
            animator.SetTrigger("Die");

        if (CoinPrefab != null)
            Instantiate(CoinPrefab, transform.position, Quaternion.identity);

        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        if (DeadVFX != null)
            Instantiate(DeadVFX, transform.position, Quaternion.identity);

        // ✅ Phát âm thanh HUỶ trước, rồi Destroy sau theo độ dài clip
        float destroyDelay = 0f;
        if (Sound_Destroy != null)
        {
            if (Sound_Play == null) Sound_Play = gameObject.AddComponent<AudioSource>();
            Sound_Play.playOnAwake = false;
            Sound_Play.PlayOneShot(Sound_Destroy);
            destroyDelay = Sound_Destroy.length; // đảm bảo clip phát hết
        }

        // Huỷ object sau khi âm thanh phát xong (nếu có), nếu không thì huỷ ngay
        StartCoroutine(FlashWhileInvincible());
        Destroy(gameObject, destroyDelay);
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }
    protected override void DealDamageToPlayer()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            PlayerHealth target = player.GetComponent<PlayerHealth>();
            if (target != null)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} gây {attackDamage} damage cho Player");
            }
        }
    }

    // Gọi từ animation event khi kết thúc đòn đánh
    public void EndAttackEvent()
    {
        isAttacking = false;
        animator.SetBool("EnemyAtk", false);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void UpdateAnimator()
    {
        base.UpdateAnimator();
    }
    private IEnumerator FlashWhileInvincible()
    {
        float duration = 2f;
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