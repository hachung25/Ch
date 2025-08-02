using UnityEngine;
using Fusion;
using System.Collections;

public class PlayerHealth2 : NetworkBehaviour, IDamageable
{
    private int currentHP;
    public int maxHP = 100;

    private PlayerHealthUI2 healthUI;
    private Animator animator;

    public override void Spawned()
    {
        if (HasStateAuthority)
            currentHP = maxHP;

        healthUI = GetComponentInChildren<PlayerHealthUI2>();
        animator = GetComponentInChildren<Animator>();

        if (HasStateAuthority)
            RPC_UpdateHealthUI(currentHP, maxHP); // Gửi máu ban đầu
    }

    public void ApplyDamage(int amount)
    {
        if (!HasStateAuthority) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"💔 Máu còn lại: {currentHP}");

        // Đồng bộ UI máu cho tất cả client
        RPC_UpdateHealthUI(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Debug.Log("💀 Người chơi đã chết");
            RPC_HandleDeath();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyDamage(int amount)
    {
        ApplyDamage(amount);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateHealthUI(int hp, int maxHp)
    {
        currentHP = hp;
        maxHP = maxHp;
        UpdateHealthUI(force: true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HandleDeath()
    {
        Debug.Log("🔁 RPC_HandleDeath gọi!");

        if (animator != null)
            animator.SetBool("isDead", true); // Animation chết

        StartCoroutine(WaitAndDestroyAfterDeath());
    }

    private IEnumerator WaitAndDestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);

        if (HasStateAuthority)
            Runner.Despawn(Object);
    }

    private void UpdateHealthUI(bool force = false)
    {
        if (healthUI != null)
            healthUI.SetHealth(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (HasStateAuthority)
            ApplyDamage(amount);
        else
            RPC_ApplyDamage(amount);
    }
}
