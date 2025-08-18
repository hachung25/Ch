using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
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
    private bool isFrozen = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(FindPlayerAfterDelay());
    }

    private IEnumerator FindPlayerAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        // Nếu đã bị đóng băng hoặc không có player thì dừng animation
        if (isFrozen)
            return;

        // Nếu player null hoặc đã rơi khỏi map
        if (player == null || Mathf.Abs(player.position.y) > 100f) // ví dụ: player rơi quá xa
        {
            FreezeEnemy(); // Dừng enemy lại
            return;
        }

        FlipToFacePlayer();

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
                    Invoke(nameof(ResetAttack), 1f); // Cooldown bắn
                }
            }
        }
        else
        {
            SetAnimStates(walking: false, firing: false);
        }
    }


    void MoveTowardPlayer()
    {
        Vector2 targetPos = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    public void ShootBullet() // Gọi từ Animation Event
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<EnemyBullet>().Initialize(direction);
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

    protected virtual void FlipToFacePlayer()
    {
        if (spriteRenderer == null || player == null) return;

        bool isPlayerOnRight = player.position.x > transform.position.x;
        spriteRenderer.flipX = !isPlayerOnRight;

        // Flip firePoint theo hướng nhìn
        if (firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (isPlayerOnRight ? 1 : -1);
            firePoint.localPosition = localPos;
        }
    }

    // Gọi khi Player chết
    public void FreezeEnemy()
    {
        isFrozen = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        SetAnimStates(false, false); // Tắt trạng thái animation
        animator.enabled = false;
    }
}
