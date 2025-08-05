using UnityEngine;
using TMPro;

public class PlayerAttack : MonoBehaviour
{
    public TextMeshProUGUI damageText;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public GameObject attackVFXPrefab;
    private int damage;

    public static event System.Action<int> OnDamageReady;

    private void OnEnable()
    {
        IndexPlayerPlayGame.OnStatsLoaded += SetDamage;
    }

    private void OnDisable()
    {
        IndexPlayerPlayGame.OnStatsLoaded -= SetDamage;
    }

    private void SetDamage(int health, int damageValue)
    {
        damage = damageValue;

        if (damageText != null)
            damageText.text = damage.ToString();

        Debug.Log("Đã cập nhật Damage từ Firebase: " + damage);
        OnDamageReady?.Invoke(damage);
    }

    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        bool hit = false;

        foreach (Collider2D enemy in enemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                hit = true; // đánh trúng ít nhất 1 enemy
            }
        }

        if (hit)
        {
            SpawnAttackVFX(); // Chỉ spawn VFX nếu có enemy bị trúng
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public int GetCurrentDamage()
    {
        return damage;
    }
    public void SpawnAttackVFX()
    {
        if (attackVFXPrefab != null && attackPoint != null)
        {
            Instantiate(attackVFXPrefab, attackPoint.position, attackPoint.rotation);
        }
    }
}