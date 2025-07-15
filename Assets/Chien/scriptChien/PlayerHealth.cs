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
    
    public PlayerMovement2D playerMovement;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }
    
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
    private void Die()
    {
        isDead = true;
        Debug.Log("Player đã chết!");
      playerMovement.Dead();
        
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}

