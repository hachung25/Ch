using UnityEngine;

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
        if(DeadVFXPrefab != null)
        {
            Instantiate(DeadVFXPrefab, transform.position , Quaternion.identity);
        }
        Destroy(gameObject, 0.7f);
        LoseGame.SetActive(true);
    }

    public void Heal(int amount)
    {
        if (isDead) return;

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
