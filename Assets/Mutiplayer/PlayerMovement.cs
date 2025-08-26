using UnityEngine;
using Fusion;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Jump (giữ cảm giác cũ)")]
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool attackHeld = false;
    private bool isAttacking = false;
    private int attackIndex = 0;
    private readonly string[] attackTriggers = { "isAtk1", "isAtk2", "isAtk3", "isAtk4" };

    private float lastDirection = 1;

    // ===== INPUT chuyển từ client -> Host (StateAuthority) =====
    private float _moveX_FromClient;     // giá trị -1..1 do client gửi sang
    private bool _jumpPressedEdge;       // cờ nhảy 1-tick do client báo sang (giữ cảm giác nhảy cũ)

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Unreliable)]
    private void RPC_SetMove(float moveX)
    {
        _moveX_FromClient = Mathf.Clamp(moveX, -1f, 1f);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Unreliable)]
    private void RPC_JumpPressed()
    {
        _jumpPressedEdge = true; // đọc 1 lần ở FixedUpdateNetwork
    }

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

        // ==== INPUT CỤC BỘ (client tự mình) ====
        float moveX = Input.GetAxisRaw("Horizontal");
        RPC_SetMove(moveX); // gửi sang Host

        if (Input.GetKeyDown(KeyCode.Y))
        {
            // giữ cảm giác nhảy cũ: bấm là nhảy (nếu đang grounded bên Host)
            RPC_JumpPressed();
        }

        attackHeld = Input.GetKey(KeyCode.T);
        if (!isAttacking && attackHeld)
            StartCoroutine(PerformAttackSequence()); // trigger trên client như cũ
    }

    public override void FixedUpdateNetwork()
    {
        // Chỉ Host/StateAuthority xử lý physics để đồng bộ
        if (!Object.HasStateAuthority) return;
        if (ChatState.IsChatting) return;

        // Ground check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck ? groundCheck.position : transform.position,
            groundCheckRadius, groundLayer);

        // ===== DI CHUYỂN (dùng input do client gửi sang) =====
        float horizontalInput = _moveX_FromClient;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (horizontalInput != 0)
        {
            // Flip theo hướng chạy (giữ cách cũ bằng localScale)
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontalInput);
            transform.localScale = scale;
            lastDirection = scale.x;
            RPC_FlipDirection(lastDirection); // sync cho mọi client
        }

        // ===== NHẢY (giữ logic cũ: bấm là nhảy nếu đang grounded) =====
        if (_jumpPressedEdge && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            RPC_SetJump(true);
        }
        else
        {
            RPC_SetJump(false);
        }
        _jumpPressedEdge = false; // reset edge

        // Animator run
        RPC_SetRun(horizontalInput != 0 && isGrounded);
    }

    private IEnumerator PerformAttackSequence()
    {
        isAttacking = true;

        do
        {
            string triggerName = attackTriggers[attackIndex];

            foreach (string trig in attackTriggers)
                RPC_ResetTrigger(trig);

            RPC_PlayAnimationTrigger(triggerName);

            // chờ vào state
            yield return null;
            yield return null;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float clipLength = stateInfo.length;

            float timer = 0f;
            while (timer < clipLength * 0.9f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            attackIndex = (attackIndex + 1) % attackTriggers.Length;

        } while (attackHeld);

        // chờ thoát state
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        isAttacking = false;
    }

    // ===== RPC Animator như cũ =====
    [Rpc(RpcSources.StateAuthority | RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetRun(bool isRunning)
    {
        animator.SetBool("isRun", isRunning);
    }

    [Rpc(RpcSources.StateAuthority | RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetJump(bool isJumping)
    {
        animator.SetBool("isJump", isJumping);
    }

    [Rpc(RpcSources.StateAuthority | RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_FlipDirection(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = direction;
        transform.localScale = scale;
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
#endif
}
