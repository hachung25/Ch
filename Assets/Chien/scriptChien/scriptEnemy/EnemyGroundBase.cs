using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    //private Vector3 initialPosition;
    //public GameObject CoinPrefab;
    //protected virtual void OnEnableEnemy()
    //{
    //    if (initialPosition == Vector3.zero)
    //        initialPosition = transform.position;

    //    transform.position = initialPosition;
    //}
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
        StartCoroutine(FindPlayerAfterDelay());
        //Dem quai trong scene
        if (CompareTag("Enemy"))
        {
            EnemyManager.Instance?.RegisterEnemy();
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
        //if (CoinPrefab != null)
        //{
        //    Instantiate(CoinPrefab, transform.position, Quaternion.identity);
        //}
        
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
