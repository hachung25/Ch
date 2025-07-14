using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Di chuyen")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Nhay")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isRunning;
    private bool isGrounded;
    private bool facingRight = true;
    private Animator animator;

    private bool isAttacking = false;
    private bool attackHeld = false;
    private int attackIndex = 0;
    private readonly string[] attackTriggers = { "isAtk1", "isAtk2", "isAtk3", "isAtk4" };
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        PlayerMove();
        PlayerJump();
        PlayerAttack();


    }

    void FixedUpdate()
    {
        float currentSpeed = runSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void PlayerMove()
    {
        //Move
        moveInput = Input.GetAxisRaw("Horizontal");
        animator.SetBool("isRun", moveInput != 0);
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();
    }

    void PlayerJump()
    {

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            animator.SetBool("isJump", false);

            if (Input.GetKeyDown(KeyCode.Y))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetBool("isJump", true);
            }
        }

    }

    void PlayerAttack()
    {
       
        attackHeld = Input.GetKey(KeyCode.T);

        if (isAttacking) return; // Đang đánh thì không gọi mới

        if (attackHeld)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    private IEnumerator PerformAttackSequence()
    {
        isAttacking = true;

        do
        {
            string triggerName = attackTriggers[attackIndex];

            // Reset all triggers trước
            foreach (string trigger in attackTriggers)
            {
                animator.ResetTrigger(trigger);
            }

            // Gửi trigger hiện tại
            animator.SetTrigger(triggerName);

            // Lấy thời gian animation hiện tại
            AnimatorStateInfo stateInfo;

            // Đợi frame để Animator cập nhật trạng thái mới
            yield return null;
            yield return null;

            // Lấy thông tin animation state
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float clipLength = stateInfo.length;

            // Đợi animation chạy gần hết (90%)
            float timer = 0f;
            while (timer < clipLength * 0.9f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Chuyển sang animation tiếp theo
            attackIndex = (attackIndex + 1) % attackTriggers.Length;

        } while (attackHeld); // Lặp nếu đang giữ nút

        // Đợi animation cuối cùng kết thúc hoàn toàn
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        isAttacking = false;
    }

}

