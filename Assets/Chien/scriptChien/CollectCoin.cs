using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    private bool isMagnetActive = false;
    private Transform player;
    public float flySpeed = 5f;

    void Update()
    {
        if (isMagnetActive && player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, flySpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.position) < 0.3f)
            {
                Destroy(gameObject);
            }
        }
    }

    // 👉 Cho phép truyền player từ bên ngoài
    public void ActivateMagnet(Transform playerTransform)
    {
        isMagnetActive = true;
        player = playerTransform;
        GetComponent<Rigidbody2D>().simulated = false;
    }
}
