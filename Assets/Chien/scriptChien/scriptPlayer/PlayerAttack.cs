using System;
using UnityEngine;
using TMPro;
using Fusion;

public class PlayerAttack : NetworkBehaviour
{
    public TextMeshProUGUI damageText;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    private int damage;

    private void Start()
    {
       int Damage = PlayerPrefs.GetInt("Upgrade_Damage");
       damage = Damage;
       textdame();
    }
    
    private void OnEnable()
    {
        
        updateDamage(); // Thực hiện điều gì đó khi bật
    }

    public void updateDamage()
    {
        int Damage = PlayerPrefs.GetInt("Upgrade_Damage");
        damage = Damage;
        textdame();
    }

    public void textdame()
    {
        damageText.text = damage.ToString();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        // Gọi DealDamage từ Animation Event hoặc logic điều khiển ở đây
    }

/*    public void DealDamage()
    {
        Debug.Log("Gây sát thương!");
        if (!HasInputAuthority) return; // 👈 CHỈ người điều khiển mới gây damage

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            var networkObj = enemy.GetComponent<NetworkObject>();
            if (networkObj == null || networkObj == GetComponentInParent<NetworkObject>())
                continue; // bỏ qua bản thân

            var damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Gây dame cho: {enemy.name}");
            }
        }
    }*/




    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
