using UnityEngine;
using Fusion;
using System.Collections;

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

    // Combo attack
    private bool isAttacking = false;
    private bool attackHeld = false;
    private int attackIndex = 0;
    private readonly string[] attackTriggers = { "isAtk1", "isAtk2", "isAtk3", "isAtk4" };

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

        if (Input.GetKeyDown(KeyCode.Y))
        {
            jumpInput = true;
        }

        attackHeld = Input.GetKey(KeyCode.T);

        if (!isAttacking && attackHeld)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (horizontalInput != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontalInput);
            transform.localScale = scale;
            lastDirection = scale.x;
            RPC_FlipDirection(lastDirection);
        }

        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            RPC_PlayAnimationTrigger("Player_jump");
            jumpInput = false;
        }

        RPC_SetRun(horizontalInput != 0 && isGrounded);
    }

    private IEnumerator PerformAttackSequence()
    {
        isAttacking = true;

        do
        {
            string triggerName = attackTriggers[attackIndex];

            // Reset all triggers
            foreach (string trig in attackTriggers)
                RPC_ResetTrigger(trig);

            // Gửi trigger hiện tại
            RPC_PlayAnimationTrigger(triggerName);

            // Wait for Animator to update state
            yield return null;
            yield return null;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float clipLength = stateInfo.length;

            // Wait for 90% of the animation duration
            float timer = 0f;
            while (timer < clipLength * 0.9f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            attackIndex = (attackIndex + 1) % attackTriggers.Length;

        } while (attackHeld);

        // Wait until animation fully finishes
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        isAttacking = false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetRun(bool isRunning)
    {
        animator.SetBool("isRun", isRunning);
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
