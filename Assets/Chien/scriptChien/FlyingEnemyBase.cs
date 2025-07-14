using System.Collections;
using UnityEngine;

public class FlyingEnemyBase : MonoBehaviour
{
    [Header("Di chuyển")]
    public Transform player;
    public float speed = 3f;
    public float chaseRange = 5f;
    public float stopRange = 1.5f;

    protected Vector3 originalPosition;
    protected Animator animator;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    protected bool isReturning = false;
    protected bool canAttack = false;
    protected bool isAttacking = false;
    protected bool isDead = false;

    protected virtual void Start()
    {
        originalPosition = transform.position;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
            rb.gravityScale = 0;
    }

    protected virtual void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < chaseRange && distance > stopRange)
        {
            // Truy đuổi
            canAttack = false;
            isReturning = false;
            MoveTo(player.position);
            ResetAttackState();
        }
        else if (distance <= stopRange)
        {
            // Tới tầm tấn công
            StopMoving();
            canAttack = true;
            OnReadyToAttack();
        }
        else
        {
            // Player rời xa -> quay về
            if (!isReturning)
            {
                isReturning = true;
                ResetAttackState();
                StartCoroutine(ReturnToStart());
            }
        }

        // Quay mặt
        FaceDirectionBat(player.position.x - transform.position.x);
    }

    protected void MoveTo(Vector3 target)
    {
        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    protected void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected IEnumerator ReturnToStart()
    {
        while (Vector2.Distance(transform.position, originalPosition) > 0.1f)
        {
            MoveTo(originalPosition);
            yield return null;
        }

        StopMoving();
        isReturning = false;
    }

    /// <summary>
    /// Lật mặt enemy theo hướng Player
    /// </summary>
    protected virtual void FaceDirectionBat(float direction)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.flipX = direction > 0;
    }

    /// <summary>
    /// Reset trạng thái tấn công
    /// </summary>
    protected virtual void ResetAttackState()
    {
        isAttacking = false;
        animator.SetBool("isAtkB", false);
    }

    /// <summary>
    /// Gọi khi enemy đến tầm tấn công — class con sẽ override
    /// </summary>
    protected virtual void OnReadyToAttack() { }

    /// <summary>
    /// Gọi khi enemy chết
    /// </summary>
    public virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        StopMoving();

        // Nếu có animation chết, bật trigger ở đây (ví dụ "Die")
        if (animator != null)
        {
            animator.SetTrigger("Die"); // animator phải có parameter "Die"
        }

        // Huỷ enemy sau 1.5s (đợi anim chết)
        Destroy(gameObject);
    }
}
