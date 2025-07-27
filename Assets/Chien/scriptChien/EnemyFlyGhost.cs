using UnityEngine;
using System.Collections;

public class EnemyFlyGhost : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 2f;
    public float chaseRange = 6f;
    public float attackRange = 4f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Target")]
    public Transform player;

    private bool isAttacking = false;
    private bool facingRight = true;

    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardPlayer();
                SetAnimStates(walking: true, firing: false);
            }
            else
            {
                SetAnimStates(walking: false, firing: true);

                if (!isAttacking)
                {
                    isAttacking = true;
                    // Gọi animation Fire — animation sẽ gọi ShootBullet() qua event
                    Invoke(nameof(ResetAttack), 1f); // cooldown giữa 2 lần bắn
                }
            }
        }
        else
        {
            // Không thấy player → idle mặc định
            SetAnimStates(walking: false, firing: false);
        }
    }

    void MoveTowardPlayer()
    {
        Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        float dirX = player.position.x - transform.position.x;
        if ((dirX > 0 && !facingRight) || (dirX < 0 && facingRight))
        {
            Flip();
        }
    }

    public void ShootBullet() // Gọi từ Animation Event
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (player.position - firePoint.position).normalized;
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * 10f;

        Debug.Log("Enemy bắn đạn từ animation event.");
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void SetAnimStates(bool walking, bool firing)
    {
        animator.SetBool("EnemyWalk", walking);
        animator.SetBool("EnemyFire", firing);
    }

    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }
}
