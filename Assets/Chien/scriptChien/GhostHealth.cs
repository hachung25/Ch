using UnityEngine;
using UnityEngine.UI;

public class FlyingEnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Effects")]
    public GameObject deathEffect;

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

        // Tắt enemy (hoặc Destroy)
        Destroy(gameObject);
    }
}

