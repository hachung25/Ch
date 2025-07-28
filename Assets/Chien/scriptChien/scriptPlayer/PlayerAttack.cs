using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public TextMeshProUGUI damageText;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;

    private int damage;

    private void Start()
    {
        StartCoroutine(InitDamageWhenReady());
    }

    private IEnumerator InitDamageWhenReady()
    {
        // Chờ đến khi dữ liệu Damage được tải từ Firebase
        while (IndexPlayerPlayGame.PlayerDamageValue == 0)
            yield return null;

        damage = IndexPlayerPlayGame.PlayerDamageValue;

        if (damageText != null)
            damageText.text = damage.ToString();

        Debug.Log("Đã cập nhật Damage từ Firebase: " + damage);
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
