using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody2D), typeof(NetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    private Rigidbody2D rb;

    private bool isGrounded = false;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        // Kiểm tra có đang đứng trên mặt đất
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Nhận input trái/phải
        float moveInput = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;

        // Nhảy
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            velocity.y = jumpForce;
        }

        rb.linearVelocity = velocity;
    }
}
