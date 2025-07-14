using UnityEngine;
using UnityEngine.UI;

public class EnemyBatFly : FlyingEnemyBase, IDamageable // 👈 THÊM IDamageable
{
    [Header("Tấn công")]
    public int damageAmount = 3;

    [Header("Máu")]
    public int maxHealth = 40;
    private int currentHealth;

    [Header("UI")]
    public Slider healthBar;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.maxValue = maxHealth;

        UpdateHealthBar();
    }

    protected override void OnReadyToAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetBool("isAtkB", true);
        }
    }

    // Gọi từ animation event tại frame tấn công
    public void DealDamageEvent()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= stopRange)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damageAmount);
        }
    }
    public void EndAttackEvent()
    {
        isAttacking = false;
        animator.SetBool("isAtkB", false);
    }

    protected override void FaceDirectionBat(float direction)
    {
        base.FaceDirectionBat(direction);
    }

    // nhận damage từ player thông qua interface
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        UpdateHealthBar();

        Debug.Log($"{gameObject.name} nhận {amount} dame. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }
}
