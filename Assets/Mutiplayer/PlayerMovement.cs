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
        if (ChatState.IsChatting) return;

        if (Input.GetKeyDown(KeyCode.Y))
            jumpInput = true;

        attackHeld = Input.GetKey(KeyCode.T);

        if (!isAttacking && attackHeld)
            StartCoroutine(PerformAttackSequence());
    }

    public override void FixedUpdateNetwork()
    {
        // ⛔ Trước đây bạn chỉ cho InputAuthority chạy physics → gây giật.
        // Nên để StateAuthority (thường là Host) xử lý chuyển động.
        if (!Object.HasStateAuthority) return;
        if (ChatState.IsChatting) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float horizontalInput = 0f;

        // Lấy input cục bộ nếu chính Host đang điều khiển object này
        if (HasInputAuthority)
            horizontalInput = Input.GetAxisRaw("Horizontal");
        else
            horizontalInput = Input.GetAxisRaw("Horizontal"); // nếu bạn đã có pipeline input riêng, thay dòng này bằng giá trị nhận từ đó

        // Dùng velocity đúng của Rigidbody2D
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (horizontalInput != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontalInput);
            transform.localScale = scale;
            lastDirection = scale.x;
            RPC_FlipDirection(lastDirection); // gọi từ StateAuthority
        }

        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            RPC_SetJump(true);   // gọi từ StateAuthority
            jumpInput = false;
        }
        else
        {
            RPC_SetJump(false);  // gọi từ StateAuthority
        }

        RPC_SetRun(horizontalInput != 0 && isGrounded); // gọi từ StateAuthority
    }

    private IEnumerator PerformAttackSequence()
    {
        isAttacking = true;

        do
        {
            string triggerName = attackTriggers[attackIndex];

            foreach (string trig in attackTriggers)
                RPC_ResetTrigger(trig);          // gọi từ InputAuthority

            RPC_PlayAnimationTrigger(triggerName); // gọi từ InputAuthority

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

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        isAttacking = false;
    }

    // ===== CHỈ SỬA ATTRIBUTE 3 RPC NÀY: cho phép StateAuthority gọi =====
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

    // Hai RPC dưới đây vẫn để InputAuthority vì được gọi trong coroutine combo (client)
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
}