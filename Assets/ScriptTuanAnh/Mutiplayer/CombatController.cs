using UnityEngine;
using Fusion;

public class CombatController : NetworkBehaviour
{
    public Transform hitPoint;           // Vị trí vùng chém
    public float hitRadius = 0.5f;       // Bán kính đánh
    public LayerMask targetLayers;       // Layer mục tiêu (Player)
    public int damage = 25;

    // 📌 Hàm này được gọi từ Animation Event
    public void DealDamage()
    {
        if (!HasStateAuthority) return; // ✅ Sửa chỗ này!

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPoint.position, hitRadius, targetLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") && hit.gameObject != gameObject)
            {
                var health = hit.GetComponent<HealthHandler>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (hitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
        }
    }
}
