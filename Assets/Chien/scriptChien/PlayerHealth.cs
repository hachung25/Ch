using UnityEngine;
using System.Collections;
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

        // BẮT ĐẦU TRẠNG THÁI BẤT TỬ VÀ NHẤP NHÁY
        isInvincible = true;
        StartCoroutine(FlashWhileInvincible());

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
    private IEnumerator FlashWhileInvincible()
    {
        float duration = 0.5f;
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