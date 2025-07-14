using UnityEngine;
using System.Collections;

//public class EnemyAutoChase : MonoBehaviour
//{
//    public float patrolRange = 3f;
//    public float moveSpeed = 2f;
//    public float chaseRange = 5f;
//    public float shootCooldown = 2f;
//    public GameObject bulletPrefab;
//    public Transform firePoint;
//    public Transform player;

//    private Vector2 startPos;
//    private int direction = 1;
//    private float shootTimer = 0f;

//    private bool facingRight = true;
//    private bool isFlipping = false;

//    private void Start()
//    {
//        startPos = transform.position;
//    }

//    private void Update()
//    {
//        if (isFlipping) return;

//        bool shouldChase = false;

//        if (player != null)
//        {
//            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
//            shouldChase = distanceToPlayer <= chaseRange;
//        }

//        if (shouldChase)
//        {
//            float deltaX = player.position.x - transform.position.x;

//            // Không di chuyển nếu sát player
//            if (Mathf.Abs(deltaX) > 0.2f)
//            {
//                float dirX = Mathf.Sign(deltaX);
//                transform.Translate(Vector2.right * dirX * moveSpeed * Time.deltaTime);
//            }

//            if (Mathf.Abs(deltaX) > 0.1f)
//            {
//                if ((deltaX > 0 && !facingRight) || (deltaX < 0 && facingRight))
//                {
//                    Flip(deltaX < 0);
//                }
//            }

//            // Bắn đạn
//            shootTimer -= Time.deltaTime;
//            if (shootTimer <= 0f)
//            {
//                Shoot();
//                shootTimer = shootCooldown;
//            }
//        }
//        else
//        {
//            // Patrol trái phải
//            transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

//            if (Mathf.Abs(transform.position.x - startPos.x) >= patrolRange)
//            {
//                StartCoroutine(DelayFlip());
//            }
//        }
//    }

//    private void Shoot()
//    {
//        if (bulletPrefab != null && firePoint != null && player != null)
//        {
//            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
//            Vector2 dir = (player.position - firePoint.position).normalized;
//            bullet.GetComponent<Bullet>().Init(dir);
//        }
//    }

//    private IEnumerator DelayFlip()
//    {
//        if (isFlipping) yield break;

//        isFlipping = true;
//        yield return new WaitForSeconds(0.1f);

//        direction *= -1;
//        Flip(direction < 0);
//        isFlipping = false;
//    }

//    private void Flip(bool faceLeft)
//    {
//        Vector3 scale = transform.localScale;
//        scale.x = faceLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
//        transform.localScale = scale;

//        facingRight = !faceLeft;
//    }
//}
public class EnemyFlyViolet : FlyingEnemyBase
{
    protected override void Start()
    {
        base.Start();
        // thêm animation, tiếng, logic riêng cho Bat nếu cần
    }

    protected override void Shoot()
    {
        base.Shoot();
        // có thể chơi hiệu ứng riêng, hoặc prefab đạn đặc biệt
    }
    protected override void HandleChase()
    {
        base.HandleChase();
        // Nếu cần logic đuổi riêng (vd zigzag, pause), có thể xử lý thêm tại đây
    }
}
