using UnityEngine;
using Fusion;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Animator")]
    public Animator animator;

    private Rigidbody2D rb;

    [Networked]
    [OnChangedRender(nameof(OnSpeedChanged))]
    public float AnimatorSpeed { get; set; }

    private bool isGrounded;
    private bool jumpPressed;
    private bool attackPressed;

    // Combo System
    private readonly string[] attackTriggers = { "isAtk1", "isAtk2", "isAtk3", "isAtk4" };
    private int attackIndex = 0;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnSpeedChanged()
    {
        animator.SetFloat("Speed", AnimatorSpeed);
    }

    private void Update()
    {
        if (!HasInputAuthority) return;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        if (Input.GetKeyDown(KeyCode.T))
            attackPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        float horizontal = Input.GetAxisRaw("Horizontal");

        // Move
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        AnimatorSpeed = Mathf.Abs(rb.linearVelocity.x);

        if (horizontal != 0)
            transform.localScale = new Vector3(Mathf.Sign(horizontal), 1, 1);

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);

        // Jump
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("isJumping");
        }

        // Attack combo
        if (attackPressed)
        {
            PerformAttack();
        }

        // Reset flags
        jumpPressed = false;
        attackPressed = false;
    }

    private void PerformAttack()
    {
        string triggerName = attackTriggers[attackIndex];

        // Gửi lệnh trigger đến tất cả client
        Rpc_TriggerAttackAnimation(triggerName);

        // Tăng combo step
        attackIndex = (attackIndex + 1) % attackTriggers.Length;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void Rpc_TriggerAttackAnimation(string triggerName)
    {
        // Reset toàn bộ trigger để đảm bảo đồng bộ đúng
        foreach (var trigger in attackTriggers)
            animator.ResetTrigger(trigger);

        animator.SetTrigger(triggerName);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
