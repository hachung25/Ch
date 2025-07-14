using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Thông số máu")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Cờ trạng thái")]
    public bool isDead = false;
    public bool isInvincible = false; // miễn nhiễm tạm thời (optional)

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }
    void Update()
    {
      
    }

    /// <summary>
    /// Nhận sát thương từ enemy
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player nhận {damage} sát thương. Máu còn: {currentHealth}");

        // Trigger animation trúng đòn nếu có
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Gọi khi máu về 0
    /// </summary>
    private void Die()
    {
        isDead = true;
        Debug.Log("Player đã chết!");
        // Trigger animation chết nếu có
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        Destroy(gameObject, 1f);
       
    }

    /// <summary>
    /// Hồi máu cho player
    /// </summary>
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    /// <summary>
    /// Getter cho UI
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}

