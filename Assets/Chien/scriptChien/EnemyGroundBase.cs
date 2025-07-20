using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public abstract class EnemyGroundBase : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public float speed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1f;
    public Transform player;

    [Header("Health")]
    public int maxHealth = 100;
    protected int currentHealth;
    protected bool isDead = false;

    [Header("UI")]
    protected Slider healthSlider;

    [Header("Direction")]
    [SerializeField] protected bool isFacingRightByDefault = false;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected bool isChasing = false;
    protected bool isAttacking = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;

        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    protected virtual void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            isChasing = true;
        }
        else if (distance > chaseRange + 0.5f)
        {
            isChasing = false;
            isAttacking = false;
        }

        if (isChasing)
        {
            if (distance > attackRange)
            {
                isAttacking = false;
                MoveTowardsPlayer();
            }
            else
            {
                isAttacking = true;
                StopMoving();
            }
        }
        else
        {
            StopMoving();
        }

        UpdateAnimator();
    }

    protected virtual void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 newPos = rb.position + direction * speed * Time.deltaTime;
        rb.MovePosition(newPos);

        if (spriteRenderer != null)
        {
            bool isPlayerOnLeft = player.position.x < transform.position.x;
            spriteRenderer.flipX = isFacingRightByDefault ? isPlayerOnLeft : !isPlayerOnLeft;
        }
    }

    protected virtual void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected virtual void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("EnemyWalk", isChasing && !isAttacking);
        animator.SetBool("EnemyAtk", isAttacking);
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log($"{gameObject.name} bị trừ {damage}, còn lại: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, 0.5f);
    }

    protected virtual void DealDamageToPlayer()
    {
        // override trong class con nếu cần
    }

    public void SetHealthSlider(Slider slider)
    {
        healthSlider = slider;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
