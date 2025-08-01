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
    public GameObject CoinPrefab;
    public bool isInvincible = false;
    protected virtual void Start()
    {
        originalPosition = transform.position;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
            rb.gravityScale = 0;

        StartCoroutine(FindPlayerAfterDelay());
        if (CompareTag("Enemy"))
        {
            //EnemyManager.Instance?.RegisterEnemy();
        }
        
    }
    private IEnumerator FindPlayerAfterDelay()
    {
        yield return null; // hoặc yield return new WaitForSeconds(0.1f);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            Debug.Log("Enemy found player: " + player.name);
        }
        else
        {
            Debug.LogWarning("Enemy could NOT find Player!");
        }
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
        animator.enabled = false;
        if (isDead) return;

        isDead = true;
        StopMoving(); 
        if (CoinPrefab != null)
        {
            Instantiate(CoinPrefab, transform.position, Quaternion.identity);
        }
        GetComponent<Rigidbody2D>().simulated = false;
       // EnemyManager.Instance?.UnregisterEnemy();
        StartCoroutine(FlashWhileInvincible());
        Destroy(gameObject, 0.8f);
    }
    private IEnumerator FlashWhileInvincible()
    {
        float duration = 0.8f;
        float timer = 0f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        while (timer < duration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        sr.enabled = true;
        isInvincible = false;
    }
}
