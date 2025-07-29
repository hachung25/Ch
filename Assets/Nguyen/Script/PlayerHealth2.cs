using UnityEngine;
using Fusion;

public class PlayerHealth2 : NetworkBehaviour, IDamageable
{
    [Networked]
    public int CurrentHP { get; set; }

    public int maxHP = 100;
    private PlayerHealthUI2 healthUI;

    private int _lastSyncedHP = -1; // 👈 thêm biến để kiểm tra HP thay đổi

    public override void Spawned()
    {
        if (HasStateAuthority)
            CurrentHP = maxHP;

        healthUI = GetComponentInChildren<PlayerHealthUI2>();
        UpdateHealthUI(force: true); // 👈 ép update ban đầu
    }

    public override void FixedUpdateNetwork()
    {
        // 👇 Mỗi client kiểm tra xem máu có thay đổi không
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
        gameObject.SetActive(false);
    }

    private void UpdateHealthUI(bool force = false)
    {
        if (healthUI != null)
            healthUI.SetHealth(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount) => ApplyDamage(amount);
}
