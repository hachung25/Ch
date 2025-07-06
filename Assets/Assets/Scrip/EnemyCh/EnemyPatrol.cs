using UnityEngine;
using UnityEngine.UI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Thiết lập enemy")]
    public float patrolDistance = 5f;
    public float speed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1f;
    public int maxHealth = 3;
    public Transform player;
    public Slider healthSlider;  // Gán Slider máu vào đây (World Space)

    private int currentHealth;
    private Vector3 startPos;
    private bool isChasing = false;
    private bool movingRight = true;
    private bool isDead = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead) return;

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
        else
        {
            Patrol();
        }

        // Test chết bằng phím P
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(1);
        }

        // (Tùy chọn) Giữ thanh máu đứng yên không xoay theo enemy
        if (healthSlider != null)
        {
            healthSlider.transform.rotation = Quaternion.identity;
        }
    }

    void Patrol()
    {
        float patrolTargetX = startPos.x + (movingRight ? patrolDistance : -patrolDistance);
        Vector3 targetPos = new Vector3(patrolTargetX, startPos.y, transform.position.z);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            movingRight = !movingRight;
        }

        FaceDirection(movingRight ? 1 : -1);
        animator.SetBool("Enemy1Atack", false);
    }

    void ChasePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        float dir = player.position.x - transform.position.x;
        FaceDirection(dir);

        if (distance > attackRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            animator.SetBool("Enemy1Atack", false);
        }
        else
        {
            animator.SetBool("Enemy1Atack", true);
            Debug.Log("Enemy tấn công!");
        }
    }

    void FaceDirection(float direction)
    {
        // Flip X để quay hướng enemy
        if (direction > 0)
            spriteRenderer.flipX = true;
        else if (direction < 0)
            spriteRenderer.flipX = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("Enemy1Die", true);
        Debug.Log("Enemy đã chết!");

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        Destroy(gameObject, 1f);
    }
}
