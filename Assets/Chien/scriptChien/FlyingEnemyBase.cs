using UnityEngine;
using System.Collections;

public class FlyingEnemyBase : MonoBehaviour
{
    [Header("General Settings")]
    public float moveSpeed = 2f;
    public float patrolRange = 3f;
    public float chaseRange = 5f;

    [Header("Combat Settings")]
    public float shootCooldown = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform player;

    protected Vector2 startPos;
    protected int direction = 1;
    protected float shootTimer = 0f;
    protected bool facingRight = true;
    protected bool isFlipping = false;

    protected virtual void Start()
    {
        startPos = transform.position;
    }

    protected virtual void Update()
    {
        if (isFlipping) return;

        if (player != null && Vector2.Distance(transform.position, player.position) <= chaseRange)
        {
            HandleChase();
            HandleShoot();
        }
        else
        {
            HandlePatrol();
        }
    }

    protected virtual void HandleChase()
    {
        float deltaX = player.position.x - transform.position.x;

        if (Mathf.Abs(deltaX) > 0.2f)
        {
            float dirX = Mathf.Sign(deltaX);
            transform.Translate(Vector2.right * dirX * moveSpeed * Time.deltaTime);
        }

        if (Mathf.Abs(deltaX) > 0.1f)
        {
            if ((deltaX > 0 && !facingRight) || (deltaX < 0 && facingRight))
            {
                Flip(deltaX < 0);
            }
        }
    }

    protected virtual void HandleShoot()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (player.position - firePoint.position).normalized;
        bullet.GetComponent<Bullet>().Init(dir);
    }

    protected virtual void HandlePatrol()
    {
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - startPos.x) >= patrolRange)
        {
            StartCoroutine(DelayFlip());
        }
    }

    protected virtual IEnumerator DelayFlip()
    {
        if (isFlipping) yield break;

        isFlipping = true;
        yield return new WaitForSeconds(0.1f);
        direction *= -1;
        Flip(direction < 0);
        isFlipping = false;
    }

    protected virtual void Flip(bool faceLeft)
    {
        Vector3 scale = transform.localScale;
        scale.x = faceLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        facingRight = !faceLeft;
    }
}

