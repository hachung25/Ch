using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private int currentHealth;
    public int MaxHealth { get; private set; }

    public bool isDead = false;
    public bool isInvincible = false;

    private Animator animator;

    public PlayerMovement2D playerMovement;

    private PlayerUpgradeManager upgradeManager; 
    
    private bool hasLoaded = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        upgradeManager = FindObjectOfType<PlayerUpgradeManager>();
        currentHealth = (MaxHealth= PlayerPrefs.GetInt("Upgrade_Health"));
        
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        Debug.Log($"Player nhận {damage} sát thương. Máu còn: {currentHealth}");

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
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        MaxHealth = currentHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}