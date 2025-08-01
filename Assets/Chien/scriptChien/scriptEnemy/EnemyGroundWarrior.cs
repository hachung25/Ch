using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class EnemyGroundWarrior : EnemyGroundBase, IDamageable
{
    [Header("Thanh máu riêng")]
    public Slider overrideHealthSlider_Warrior;

    [Header("Tùy chỉnh chỉ số")]
    public int attackDamage = 5;
    private Vector3 initialPosition;
    public GameObject CoinPrefab;

    public bool isInvincible = false;
    protected override void Start()
    {
        base.Start();

        currentHealth = maxHealth;

        if (overrideHealthSlider_Warrior != null)
        {
            healthSlider = overrideHealthSlider_Warrior;
            healthSlider.maxValue = maxHealth;
        }

        UpdateHealthBar();
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

    protected override void Die()
    {
        animator.enabled = false;
        if (isDead) return;
        isDead = true;
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        if (animator != null)
            animator.SetTrigger("Die");
        if (CoinPrefab != null)
        {
            Instantiate(CoinPrefab, transform.position, Quaternion.identity);
        }
        GetComponent<Rigidbody2D>().simulated = false;
        Debug.Log($"{gameObject.name} đã chết!");
       // EnemyManager.Instance?.UnregisterEnemy();
        StartCoroutine(FlashWhileInvincible());
        Destroy(gameObject, 0.8f);
    }

    private IEnumerator FlashWhileInvincible()
    {
        float duration = 0.8f;
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

   public void OnEnableEnemy()
    {
        if (initialPosition == Vector3.zero)
          initialPosition = transform.position;
               transform.position = initialPosition;
    }
}
