using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletRainProjectile : MonoBehaviour
{
    [Header("Move")]
    public float initialDownSpeed = 14f;
    public float maxLifetime = 4f;
    [Tooltip("Sprite mặc định quay sang phải? (false = quay trái)")]
    public bool spriteFacesRightAtZero = false;

    [Header("Damage (Enemy)")]
    public int damage = 25;
    public LayerMask enemyLayers;
    public bool destroyOnEnemyHit = true;
    [Min(1)] public int pierceCount = 1; // 1 = không xuyên

    [Header("VFX khi trúng Enemy")]
    public GameObject enemyHitVFX;
    public float enemyVfxLifetime = 1.0f;

    [Header("Environment (Ground)")]
    public LayerMask groundLayers;
    public GameObject groundHitVFX;
    public float groundVfxLifetime = 1.2f;

    float life;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.down * initialDownSpeed;
        rb.freezeRotation = true;

        Vector2 dir = rb.linearVelocity.normalized;
        transform.right = spriteFacesRightAtZero ? dir : -dir; // sprite mặc định quay trái -> dùng -dir
    }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= maxLifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Vector2 point = other.ClosestPoint(transform.position);
        TryResolveHit(other.gameObject, point);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Vector2 point = col.contactCount > 0 ? col.GetContact(0).point : (Vector2)transform.position;
        TryResolveHit(col.collider.gameObject, point);
    }

    void TryResolveHit(GameObject other, Vector2 hitPoint)
    {
        bool isEnemy = (enemyLayers.value & (1 << other.layer)) != 0;
        bool isGround = (groundLayers.value & (1 << other.layer)) != 0;

        // 1) Enemy: gây damage + nổ
        if (isEnemy)
        {
            var dmg = other.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage);

            SpawnVFX(enemyHitVFX, hitPoint, enemyVfxLifetime);

            pierceCount--;
            if (destroyOnEnemyHit && pierceCount <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 2) Ground: nổ + huỷ
        if (isGround)
        {
            SpawnVFX(groundHitVFX, hitPoint, groundVfxLifetime);
            Destroy(gameObject);
            return;
        }
    }

    void SpawnVFX(GameObject prefab, Vector2 at, float lifetime)
    {
        if (!prefab) return;
        var vfx = Instantiate(prefab, at, Quaternion.identity);
        if (lifetime > 0f) Destroy(vfx, lifetime);
    }
}
