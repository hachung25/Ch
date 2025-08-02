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

    // Sự kiện khi damage đã sẵn sàng
    public static event System.Action<int> OnDamageReady;

    private void Start()
    {
        StartCoroutine(InitDamageWhenReady());
    }

    private IEnumerator InitDamageWhenReady()
    {
        // Chờ dữ liệu từ Firebase được tải xong
        while (!IndexPlayerPlayGame.IsLoaded)
            yield return null;

        damage = IndexPlayerPlayGame.PlayerDamageValue;

        if (damageText != null)
            damageText.text = damage.ToString();

        Debug.Log("Đã cập nhật Damage từ Firebase: " + damage);

        OnDamageReady?.Invoke(damage); // Gửi thông báo cho các hệ thống khác nếu cần
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

    // Hàm public nếu muốn lấy damage từ bên ngoài
    public int GetCurrentDamage()
    {
        return damage;
    }
}