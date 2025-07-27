using System;
using UnityEngine;
using TMPro;

public class PlayerAttack : MonoBehaviour
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

    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
