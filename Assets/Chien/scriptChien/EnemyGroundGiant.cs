using UnityEngine;
using UnityEngine.UI;

public class EnemyGroundGiant : EnemyGroundBase, IDamageable
{
    [Header("Thanh máu riêng")]
    public Slider overrideHealthSlider_Giant;

    [Header("Tùy chỉnh chỉ số")]
    public int attackDamage = 8;

    protected override void Start()
    {
        base.Start();

        currentHealth = maxHealth;

        if (overrideHealthSlider_Giant != null)
        {
            healthSlider = overrideHealthSlider_Giant;
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
        if (isDead) return;

        isDead = true;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        if (animator != null)
            animator.SetTrigger("Die");

        Debug.Log($"{gameObject.name} đã chết!");
        Destroy(gameObject, 0.3f);
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


}
