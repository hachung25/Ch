using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections;

public class HealthHandler : NetworkBehaviour
{
    [Networked]
    public int Health { get; set; } = 100;

    public Slider healthSlider;       // Gán trong Inspector
    public Animator animator;         // Gán Animator nhân vật
    private int lastHealth = -1;      // Dùng để so sánh tránh update liên tục

    public override void Spawned()
    {
        lastHealth = Health;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"💥 TakeDamage called on {gameObject.name} | HasStateAuthority: {HasStateAuthority}");

        if (!HasStateAuthority) return;
        if (Health <= 0) return;

        Health -= amount;
        Debug.Log($"❤️ {gameObject.name} new Health: {Health}");

        if (Health <= 0)
        {
            Health = 0;
            if (animator != null)
            {
                animator.SetBool("isDeal", true);
            }
            StartCoroutine(HandleDeath());
        }
    }

    private IEnumerator HandleDeath()
    {
        Debug.Log($"💀 {gameObject.name} is dying in 3 seconds...");
        yield return new WaitForSeconds(3f);
        if (HasStateAuthority && Object != null)
        {
            Runner.Despawn(Object);
        }
    }

    public override void Render()
    {
        if (lastHealth != Health)
        {
            lastHealth = Health;
            UpdateHealthUI();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Health / 100f;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no healthSlider assigned!");
        }
    }
}
