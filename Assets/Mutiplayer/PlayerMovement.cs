using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool jumpInput = false;
    private float lastDirection = 1;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 2;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (!HasInputAuthority) return;

        // Nhảy chỉ cần nhấn 1 lần nên lưu lại
        if (Input.GetKeyDown(KeyCode.Y))
        {
            jumpInput = true;
        }

        // Tấn công (gửi RPC)
        if (Input.GetKeyDown(KeyCode.T))
        {
            RPC_PlayAnimation("Player_atk1");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        // Kiểm tra mặt đất
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Di chuyển
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Lật hướng nhân vật
        if (horizontalInput != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontalInput);
            transform.localScale = scale;
            lastDirection = scale.x;

            RPC_FlipDirection(lastDirection); // Gửi hướng sang client khác
        }

        // Nhảy
        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            RPC_PlayAnimation("Player_jump");
            jumpInput = false;
        }

        // RPC chạy Idle/Run
        if (horizontalInput != 0 && isGrounded)
        {
            RPC_SetRun(true);
        }
        else
        {
            RPC_SetRun(false);
        }
    }

    // RPC để bật/tắt animation chạy
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetRun(bool isRunning)
    {
        animator.SetBool("isRun", isRunning);
    }

    // RPC để play animation (attack, jump...)
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayAnimation(string animName)
    {
        animator.Play(animName);
    }

    // RPC để lật hướng
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_FlipDirection(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = direction;
        transform.localScale = scale;
    }

    /*public void DealDamage()
    {
        Debug.Log("Gây sát thương!");
        // logic tấn công
    }*/

}
