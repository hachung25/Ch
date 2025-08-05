using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 7;
    public float lifeTime = 5f;
    public GameObject hitEffect;

    private Vector2 targetDirection;

    public void Initialize(Vector2 direction)
    {
        targetDirection = direction.normalized;
        RotateTowardsDirection(targetDirection);
    }

    private void RotateTowardsDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * Time.deltaTime * 10f); // giả sử tốc độ là 10
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) return;

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
