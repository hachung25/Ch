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

    // ======== Thêm: cache input từ client gửi sang server =========
    // (chỉ StateAuthority dùng các biến này để simulate)
    private float _srvHorizontal = 0f;
    private bool _srvJumpPressed = false;
    private bool _srvAttackHeld = false;

    // ======== Thêm: tránh spam RPC animator/flip =========
    private bool _lastSentIsRun = false;
    private bool _lastSentIsJump = false;
    private float _lastSentFacing = 1f;

    // ======== Thêm: tránh spam RPC_Input =========
    private float _lastSentHorizontal = 0f;
    private bool _lastSentAttackHeld = false;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 2;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (lastDirection == 0) lastDirection = 1f;
        _lastSentFacing = lastDirection;
    }

    private void Update()
    {
        if (!HasInputAuthority) return;

        // 🚫 Ngăn input khi đang chat
        if (ChatState.IsChatting) return;

        // --- Input local ---
        if (Input.GetKeyDown(KeyCode.Y))
        {
            jumpInput = true; // edge
        }

        attackHeld = Input.GetKey(KeyCode.T);

        // Gửi input chuyển động gọn sang StateAuthority khi có thay đổi đáng kể
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        bool needSend =
            Mathf.Abs(horizontalInput - _lastSentHorizontal) > 0.01f ||
            attackHeld != _lastSentAttackHeld ||
            jumpInput; // jump là edge, gửi ngay khi nhấn

        if (needSend)
        {
            RPC_InputToServer(horizontalInput, jumpInput, attackHeld);
            _lastSentHorizontal = horizontalInput;
            _lastSentAttackHeld = attackHeld;
            jumpInput = false; // đã gửi edge -> reset
        }

        // Logic combo (giữ nguyên): client điều khiển, phát trigger qua RPC khi cần
        if (!isAttacking && attackHeld)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    public override void FixedUpdateNetwork()
    {
        // ❗❗ Chỉ Server/Host (StateAuthority) mới simulate physics
        if (!Object.HasStateAuthority) return;

        // Ground check ở server để trạng thái nhất quán
        if (groundCheck)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Move theo input đã nhận từ client
        var vel = rb.linearVelocity;
        vel.x = _srvHorizontal * moveSpeed;
        rb.linearVelocity = vel;

        // Facing khi có hướng
        if (Mathf.Abs(_srvHorizontal) > 0.001f)
        {
            float newFace = Mathf.Sign(_srvHorizontal);
            if (newFace != _lastSentFacing)
            {
                _lastSentFacing = newFace;
                lastDirection = newFace;
                RPC_FlipDirection(lastDirection); // chỉ khi đổi hướng
            }
        }

        // Jump (edge-trigger do client gửi)
        if (_srvJumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        // reset edge đã dùng
        _srvJumpPressed = false;

        // Animator states (gửi khi đổi)
        bool nowRun = Mathf.Abs(_srvHorizontal) > 0.01f && isGrounded;
        bool nowJump = !isGrounded; // hoặc tùy bạn muốn hiển thị isJump lúc nào

        if (nowRun != _lastSentIsRun)
        {
            _lastSentIsRun = nowRun;
            RPC_SetRun(nowRun);
        }

        if (nowJump != _lastSentIsJump)
        {
            _lastSentIsJump = nowJump;
            RPC_SetJump(nowJump);
        }
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

            // chờ 2 frame render để animator vào state
            yield return null;
            yield return null;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float clipLength = stateInfo.length;

            float timer = 0f;
            while (timer < clipLength * 0.9f)
            {
                timer += Time.deltaTime; // chấp nhận ở client cho nhịp combo
                yield return null;
            }

            attackIndex = (attackIndex + 1) % attackTriggers.Length;

        } while (attackHeld);

        // đợi state hoàn tất
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        isAttacking = false;
    }

    // ======== RPCs gốc (giữ nguyên) =========

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

    // ======== Thêm: RPC gửi input từ client -> server =========
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_InputToServer(float horizontal, bool jumpPressedEdge, bool attackHeldNow)
    {
        _srvHorizontal = horizontal;
        _srvAttackHeld = attackHeldNow;
        // jump là edge, chỉ dùng 1 tick server
        _srvJumpPressed = _srvJumpPressed || jumpPressedEdge;
    }
}
