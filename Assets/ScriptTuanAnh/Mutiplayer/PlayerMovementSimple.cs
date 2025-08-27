using UnityEngine;
using Fusion;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovementSimple : NetworkBehaviour
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

    // Attack
    private bool attackHeld = false;
    private bool isAttacking = false;
    private const string attackTrigger = "isAtk1";

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
        if (ChatState.IsChatting) return;

        if (Input.GetKeyDown(KeyCode.Y))
        {
            jumpInput = true;
        }

        attackHeld = Input.GetKeyDown(KeyCode.T);

        if (attackHeld && !isAttacking)
        {
            StartCoroutine(DoSingleAttack());
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (ChatState.IsChatting) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (horizontal != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontal);
            transform.localScale = scale;
            lastDirection = scale.x;
            RPC_FlipDirection(lastDirection);
        }

        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            RPC_SetJump(true);
            jumpInput = false;
        }
        else
        {
            RPC_SetJump(false);
        }

        RPC_SetRun(horizontal != 0 && isGrounded);
    }

    private IEnumerator DoSingleAttack()
    {
        isAttacking = true;

        RPC_ResetTrigger(attackTrigger);
        RPC_PlayAnimationTrigger(attackTrigger);

        // Đợi gần hết animation
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float duration = stateInfo.length;
        yield return new WaitForSeconds(duration * 0.9f);

        isAttacking = false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetRun(bool isRunning)
    {
        animator.SetBool("isRun", isRunning);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetJump(bool isJumping)
    {
        animator.SetBool("isJump", isJumping);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayAnimationTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_ResetTrigger(string trigger)
    {
        animator.ResetTrigger(trigger);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_FlipDirection(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = direction;
        transform.localScale = scale;
    }
}
