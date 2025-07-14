using UnityEngine;

public class EnemyPatrol : EnemyBasePatrol
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool inChaseRange = distanceToPlayer <= chaseRange;
        bool inAttackRange = distanceToPlayer <= attackRange;

        isChasing = inChaseRange;

        // Cập nhật Animator theo trạng thái
        if (animator != null)
        {
            animator.SetBool("Idle", !isChasing);
            animator.SetBool("EnemyWalk", isChasing && !inAttackRange);
            animator.SetBool("EnemyAtk", isChasing && inAttackRange);
        }

        if (isChasing)
        {
            ChasePlayer();
        }
    }

    protected override void ChasePlayer()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        if (distance > attackRange)
        {
            // Di chuyển tới player
            Vector2 newPos = rb.position + direction * speed * Time.deltaTime;
            rb.MovePosition(newPos);
        }
        else
        {
            // Chuẩn bị tấn công → reset sát thương mới
            hasHitPlayer = false;
        }

        FaceDirection(direction.x);
    }

    protected override void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Enemy1Die");
            animator.SetBool("Idle", false);
            animator.SetBool("EnemyWalk", false);
            animator.SetBool("EnemyAtk", false);
        }

        base.Die();
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        // TODO: Thêm hiệu ứng bị đánh nếu cần
    }

    protected override Transform GetPatrolTarget() => null;
    protected override void SwitchPatrolTarget() { }
}
