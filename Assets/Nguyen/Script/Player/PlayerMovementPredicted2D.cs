using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerMovementPredicted2D : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visual;        // child: SpriteRenderer + Animator
    [SerializeField] private Animator animator;       // Animator trên visual
    [SerializeField] private Transform groundCheck;   // empty dưới chân

    [Header("Move")]
    public float moveSpeed = 6f;
    public float accelGround = 40f;
    public float accelAir = 20f;
    public float maxFallSpeed = -20f;

    [Range(0f, 1f)]
    public float inputSmoothing = 0.25f;   // mượt input ngang

    [Header("Jump")]
    public float jumpForce = 9f;
    public float coyoteMs = 120f;   // nhảy ngay sau khi rời đất
    public float jumpBufferMs = 100f;   // bấm sớm vẫn ăn

    [Header("Ground")]
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float groundRayLen = 0.4f;    // ray lấy normal để chạy theo slope

    [Header("Combo")]
    public int comboCadenceMs = 220;     // nhịp auto-combo khi giữ T

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private bool isGrounded;
    private bool wasGrounded;
    private Vector2 lastGroundNormal = Vector2.up;

    // ====== Networked visual state ======
    [Networked] private float FacingX { get; set; }   // -1/1
    [Networked] private NetworkBool RunNw { get; set; }
    [Networked] private NetworkBool JumpNw { get; set; }   // server set: true khi đang trên không

    // Combo: index & serial
    [Networked] private int AttackIndex { get; set; }   // 0..3
    [Networked] private int AttackSerial { get; set; }   // +1 mỗi đòn

    // NEW: JumpStart serial (fire trigger đúng 1 lần / take-off)
    [Networked] private int JumpSerial { get; set; }   // +1 khi take-off

    // Auto-combo timer (server)
    [Networked] private TickTimer ComboTimer { get; set; }

    // Client predicted hiển thị
    private bool _predictedJump = false;

    // Local tracking
    private int _lastAttackSerial = 0;
    private int _lastJumpSerial = 0;

    private TickTimer _jumpBufferTimer;   // local
    private TickTimer _coyoteTimer;       // local
    private float _smoothedInputX = 0f;

    // Animator params/trigger
    private readonly string[] atkTriggers = { "isAtk1", "isAtk2", "isAtk3", "isAtk4" };
    private const string RUN_BOOL = "isRun";
    private const string JUMP_BOOL = "isJump";
    private const string JUMP_TRIG = "isJumpStart";  // <-- tạo trigger này trong Animator

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!visual) Debug.LogError("[Player] Missing visual (child). Drag the child Transform here.");
        if (!animator) animator = visual ? visual.GetComponent<Animator>() : null;
        if (!animator) Debug.LogError("[Player] Missing Animator on visual.");
        sprite = visual ? visual.GetComponentInChildren<SpriteRenderer>() : null;

        rb.interpolation = RigidbodyInterpolation2D.None; // dùng NetworkTransform để interp
        rb.gravityScale = 2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (FacingX == 0) FacingX = 1;
        lastGroundNormal = Vector2.up;
    }

    public override void FixedUpdateNetwork()
    {
        float dt = Runner.DeltaTime;

        // ---- Ground check ----
        wasGrounded = isGrounded;

        // Nếu dự án bạn không có GetPhysicsScene2D(), đổi sang Physics2D.OverlapCircle(...)
        isGrounded = Runner.GetPhysicsScene2D()
            .OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        var hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundRayLen, groundLayer);
        if (hit.collider) lastGroundNormal = hit.normal;
        else if (isGrounded) lastGroundNormal = Vector2.up;

        // Coyote
        if (isGrounded)
        {
            _coyoteTimer = TickTimer.None;
        }
        else if (wasGrounded && !isGrounded && !_coyoteTimer.IsRunning)
        {
            _coyoteTimer = TickTimer.CreateFromSeconds(Runner, coyoteMs / 1000f);
        }

        // ---- Input + Movement ----
        if (GetInput(out PlayerInputData input))
        {
            // Smooth input ngang
            _smoothedInputX = Mathf.Lerp(
                _smoothedInputX,
                Mathf.Clamp(input.Horizontal, -1f, 1f),
                1f - Mathf.Pow(1f - inputSmoothing, 60f * dt)
            );

            // Target velocity (slope-aware khi đứng đất)
            Vector2 targetVel;
            if (isGrounded && lastGroundNormal.y > 0.1f)
            {
                Vector2 tangent = new Vector2(lastGroundNormal.y, -lastGroundNormal.x).normalized;
                targetVel = tangent * (_smoothedInputX * moveSpeed);
            }
            else
            {
                targetVel = new Vector2(_smoothedInputX * moveSpeed, rb.linearVelocity.y);
            }

            float accel = isGrounded ? accelGround : accelAir;

            float newVX = Mathf.MoveTowards(rb.linearVelocity.x, targetVel.x, accel * dt);
            float newVY = rb.linearVelocity.y;

            // Ground-stick
            if (isGrounded && newVY <= 0f)
                newVY = Mathf.Max(newVY, -3f);

            // Jump buffer
            if (input.JumpPressed)
                _jumpBufferTimer = TickTimer.CreateFromSeconds(Runner, jumpBufferMs / 1000f);

            bool wantJump = _jumpBufferTimer.IsRunning;

            // ---- TAKE-OFF ----
            if (wantJump && (isGrounded || _coyoteTimer.IsRunning))
            {
                newVY = jumpForce;
                _jumpBufferTimer = TickTimer.None;
                _coyoteTimer = TickTimer.None;

                // 1) Hiển thị ngay trên máy người chơi
                if (HasInputAuthority) _predictedJump = true;

                // 2) Đồng bộ: server tăng JumpSerial để mọi máy phát trigger ngay
                if (Object.HasStateAuthority)
                    JumpSerial++;
            }

            // Limit rơi
            if (newVY < maxFallSpeed) newVY = maxFallSpeed;

            rb.linearVelocity = new Vector2(newVX, newVY);

            // Facing (flip sprite — không đổi scale root)
            if (Mathf.Abs(_smoothedInputX) > 0.001f)
                FacingX = Mathf.Sign(_smoothedInputX);

            // ----- COMBO -----
            if (Object.HasStateAuthority && input.AttackPressed)
            {
                AttackIndex = (AttackIndex + 1) % atkTriggers.Length;
                AttackSerial++;
            }
            if (Object.HasStateAuthority && input.AttackHeld)
            {
                if (!ComboTimer.IsRunning)
                {
                    AttackIndex = (AttackIndex + 1) % atkTriggers.Length;
                    AttackSerial++;
                    ComboTimer = TickTimer.CreateFromSeconds(Runner, comboCadenceMs / 1000f);
                }
            }
            else if (Object.HasStateAuthority && ComboTimer.IsRunning && !input.AttackHeld)
            {
                ComboTimer = TickTimer.None;
            }
        }

        // Server ghi state animator (mọi máy xem)
        if (Object.HasStateAuthority)
        {
            RunNw = Mathf.Abs(rb.linearVelocity.x) > 0.01f && isGrounded;
            JumpNw = !isGrounded; // true khi đang trên không
        }

        // Khi về đất → tắt predicted jump local
        if (isGrounded) _predictedJump = false;
    }

    public override void Render()
    {
        // Flip bằng SpriteRenderer.flipX
        if (sprite)
            sprite.flipX = FacingX < 0f;

        if (!animator) return;

        // Hiển thị nhảy mượt: OR giữa net và predicted local
        bool isJumpShown = JumpNw || _predictedJump;

        // —— Phát JUMP START trigger khi server báo take-off ——
        if (_lastJumpSerial != JumpSerial)
        {
            // reset rồi bắn trigger JumpStart
            animator.ResetTrigger(JUMP_TRIG);
            animator.SetTrigger(JUMP_TRIG);
            isJumpShown = true; // đảm bảo bool cũng true
            _lastJumpSerial = JumpSerial;
        }

        animator.SetBool(RUN_BOOL, RunNw);
        animator.SetBool(JUMP_BOOL, isJumpShown);

        // —— Combo triggers theo AttackSerial ——
        if (_lastAttackSerial != AttackSerial)
        {
            foreach (var t in atkTriggers) animator.ResetTrigger(t);
            string trig = atkTriggers[Mathf.Clamp(AttackIndex, 0, atkTriggers.Length - 1)];
            animator.SetTrigger(trig);
            _lastAttackSerial = AttackSerial;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundRayLen);
        }
    }
#endif
}
