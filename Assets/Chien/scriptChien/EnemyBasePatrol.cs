using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBasePatrol : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    public float speed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1f;
    public int maxHealth = 40;
    public Transform player;

    [Header("UI")]
    public Slider healthSlider;

    protected int currentHealth;
    protected bool isChasing = false;
    protected bool isDead = false;
    protected bool hasHitPlayer = false;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        rb.freezeRotation = true;

        // Khởi tạo slider (nếu có)
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
        {
            isChasing = true;
        }
        else if (distanceToPlayer > chaseRange + 1f)
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
    }

    protected virtual void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            Vector2 move = direction * speed * Time.deltaTime;
            rb.MovePosition(rb.position + move);

            if (animator != null)
                animator.SetBool("EnemyAtk", false);

            hasHitPlayer = false;
        }
        else
        {
            if (animator != null)
                animator.SetBool("EnemyAtk", true);
        }

        FaceDirection(direction.x);
    }

    protected void FaceDirection(float direction)
    {
        if (direction > 0)
            spriteRenderer.flipX = true;
        else if (direction < 0)
            spriteRenderer.flipX = false;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Cập nhật thanh máu
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} đã chết!");

        // Ẩn thanh máu nếu muốn
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }

    public void DealDamageToPlayer()
    {
        if (isDead || hasHitPlayer || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            player.GetComponent<IDamageable>()?.TakeDamage(50);
            hasHitPlayer = true;
        }
    }

    protected abstract Transform GetPatrolTarget();
    protected abstract void SwitchPatrolTarget();
}
