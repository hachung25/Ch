using UnityEngine;
using Fusion;

public class PlayerMovement : NetworkBehaviour
{
    public Rigidbody2D rb2d;
    public float speed = 5f;

    public override void FixedUpdateNetwork()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);
        rb2d.MovePosition(rb2d.position + move * speed * Runner.DeltaTime);
    }
}
