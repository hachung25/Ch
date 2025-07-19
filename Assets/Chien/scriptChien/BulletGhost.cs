using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 7;
    public float lifeTime = 5f;
    public GameObject hitEffect;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) return; // tránh bắn trúng chính nó

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

}
