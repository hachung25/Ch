using UnityEngine;

public class EnemyLookAtPlayer : MonoBehaviour
{
    public Transform player; // Gán trong Inspector hoặc tự tìm bằng tag
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }
}

