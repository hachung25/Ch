using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class FlyingEnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Effects")]
    public GameObject deathEffect;
    public GameObject CoinPrefab;
    public bool isInvincible = false;
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
        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        Debug.Log($"Enemy nhận {damage} dame từ: {name}");
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
        // Hiệu ứng chết
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        if(CoinPrefab != null)
        {
            Instantiate(CoinPrefab, transform.position, Quaternion.identity);
        }
        GetComponent<Rigidbody2D>().simulated = false;
   //    EnemyManager.Instance?.UnregisterEnemy();
        StartCoroutine(FlashWhileInvincible());
        Destroy(gameObject,0.8f);
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

