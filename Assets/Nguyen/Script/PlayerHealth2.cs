using UnityEngine;
using Fusion;
using System.Collections;

public class PlayerHealth2 : NetworkBehaviour, IDamageable
{
    [Networked] public int CurrentHP { get; set; }

    public int maxHP = 100;
    private PlayerHealthUI2 healthUI;
    private Animator animator;

    private int _lastSyncedHP = -1;

    public override void Spawned()
    {
        if (HasStateAuthority)
            CurrentHP = maxHP;

        healthUI = GetComponentInChildren<PlayerHealthUI2>();
        animator = GetComponentInChildren<Animator>();
        UpdateHealthUI(force: true);
    }

    public override void FixedUpdateNetwork()
    {
        if (_lastSyncedHP != CurrentHP)
        {
            _lastSyncedHP = CurrentHP;
            UpdateHealthUI(force: true);
        }
    }

    public void ApplyDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Debug.Log("💔 Máu còn: " + CurrentHP);

        if (CurrentHP <= 0)
        {
            Debug.Log("💀 Chết");
            RPC_HandleDeath();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyDamage(int amount)
    {
        ApplyDamage(amount);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HandleDeath()
    {
        Debug.Log("🔁 RPC_HandleDeath gọi!");

        if (animator != null)
        {
            animator.SetBool("isDead", true); // Kích hoạt animation chết
        }

        StartCoroutine(WaitAndDestroyAfterDeath());
    }

    private IEnumerator WaitAndDestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f); // ⏳ Đợi 2 giây để animation chơi xong

        if (HasStateAuthority)
        {
            Runner.Despawn(Object); // ✅ Xoá đúng cách trong Fusion
        }
    }

    private void UpdateHealthUI(bool force = false)
    {
        if (healthUI != null)
        {
            healthUI.SetHealth(CurrentHP, maxHP);
        }
    }

    public void TakeDamage(int amount) => ApplyDamage(amount);
}
