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

    private PlayerStats myStats;

    private void Start()
    {
        damage = PlayerPrefs.GetInt("Upgrade_Damage", damage);
        UpdateDamageText();
        myStats = GetComponent<PlayerStats>();
    }

    private void OnEnable() => UpdateDamageText();

    private void UpdateDamageText()
    {
        if (damageText != null)
            damageText.text = damage.ToString();
    }

    // Gọi từ Animation Event (ví dụ trong Player_atk1)
    public void DealDamage()
    {
        if (!HasInputAuthority) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            var netObj = hit.GetComponent<NetworkObject>();
            if (netObj != null && netObj.TryGetBehaviour<PlayerHealth2>(out var health))
            {
                Debug.Log("🎯 Gửi sát thương tới " + hit.name);
                health.TakeDamageFrom(myStats, damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
