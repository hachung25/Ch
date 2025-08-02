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
    public GameObject LoseGame;
    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("PlayerHealth Start gọi InitHealthWhenReady()");
        StartCoroutine(InitHealthWhenReady());
    }

    private IEnumerator InitHealthWhenReady()
    {
        float timeout = 5f; // thời gian chờ tối đa
        float timer = 0f;

        Debug.Log("Đang chờ Firebase load Health...");

        while (IndexPlayerPlayGame.PlayerHealthValue == 0 && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (IndexPlayerPlayGame.PlayerHealthValue == 0)
        {
            Debug.LogError("Không thể lấy máu từ Firebase sau 5 giây. Gán mặc định 100.");
            MaxHealth = 100; // fallback
        }
        else
        {
            MaxHealth = IndexPlayerPlayGame.PlayerHealthValue;
            Debug.Log("Máu đã được gán từ Firebase: " + MaxHealth);
        }

        currentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
     

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        Debug.Log($"Player nhận {damage} sát thương. Máu còn: {currentHealth}");

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
    private IEnumerator FlashWhileInvincible()
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
