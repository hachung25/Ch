using UnityEngine;
using Fusion;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        //if (!Object.HasInputAuthority) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical).normalized;

        rb.MovePosition(rb.position + move * speed * Runner.DeltaTime);
    }
}
