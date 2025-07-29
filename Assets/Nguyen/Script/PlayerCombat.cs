using UnityEngine;
using Fusion;
using TMPro;

public class PlayerCombat : NetworkBehaviour
{
    public int damage = 20;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public TextMeshProUGUI damageText;

    private void Start()
    {
        damage = PlayerPrefs.GetInt("Upgrade_Damage", damage);
        UpdateDamageText();
    }

    private void OnEnable() => UpdateDamageText();

    private void UpdateDamageText()
    {
        if (damageText != null)
            damageText.text = damage.ToString();
    }

    // Gọi hàm này từ animation event
    public void DealDamage()
    {
        if (!HasInputAuthority) return;

        Debug.Log("⛏ Gây sát thương!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            var netObj = hit.GetComponent<NetworkObject>();
            if (netObj != null && netObj.TryGetBehaviour<PlayerHealth2>(out var health))
            {
                Debug.Log("🎯 Gửi sát thương tới " + hit.name);
                health.RPC_ApplyDamage(damage);
            }
        }

    }



    [Rpc(RpcSources.InputAuthority, RpcTargets.InputAuthority)]
    public void RPC_RequestDamage(NetworkObject target, int amount)
    {
        if (target != null && target.TryGetBehaviour<PlayerHealth2>(out var health))
        {
            health.ApplyDamage(amount);
        }
    }




    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
