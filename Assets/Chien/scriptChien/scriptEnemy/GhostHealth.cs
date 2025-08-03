using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GhostHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 30;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Effects")]
    public GameObject deathEffect;
    public GameObject CoinPrefab;
    public bool isInvincible = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.minValue = 0;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        UpdateHealthUI();

        Debug.Log($"Enemy nhận {damage} dame từ: {name}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Spawn death effects
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        if (CoinPrefab != null)
        {
            Instantiate(CoinPrefab, transform.position, Quaternion.identity);
        }

        // Disable components
        if (anim != null)
            anim.enabled = false;

        if (col != null)
            col.enabled = false;

        if (rb != null)
            rb.simulated = false;

        StartCoroutine(FlashWhileInvincible());

        // Destroy after short delay
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
}
