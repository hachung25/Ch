using UnityEngine;
using Fusion;
using System.Collections;
using System.Linq;

public class PlayerHealth2 : NetworkBehaviour, IDamageable
{
    // ===== HP =====
    [SerializeField] private int currentHP;
    public int maxHP = 100;

    // ===== UI & Animator =====
    private PlayerHealthUI2 healthUI;
    private Animator animator;

    // ===== Respawn / Death Flow =====
    [Header("Respawn Settings")]
    public Transform respawnPoint;         // Kéo vị trí hồi sinh vào Inspector
    public float respawnDelay = 3f;        // Thời gian chờ hồi sinh (3s)

    [Header("Death Animation")]
    public float deathAnimDelay = 1f;      // Đợi 1s rồi mới vào anim Die
    public string dieStateName = "Die";    // Tên state animation chết
    public int dieLayer = 0;               // Layer chứa state chết
    [Range(0f, 1f)] public float hideAtNormalized = 0.95f; // Ẩn khi anim gần xong
    public bool respawnAfterAnim = true;   // true: bắt đầu đếm respawn SAU khi anim die xong

    // ===== Caches để bật/tắt nhanh =====
    private Renderer[] renderers;
    private Collider2D[] colliders2D;
    private Collider[] colliders3D;
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;

    // Tự động gom các script có khả năng là điều khiển chuyển động/nhập liệu để disable lúc chết
    private MonoBehaviour[] movementScripts;

    public override void Spawned()
    {
        if (HasStateAuthority)
            currentHP = maxHP;

        healthUI = GetComponentInChildren<PlayerHealthUI2>();
        animator = GetComponentInChildren<Animator>();

        // Cache comps
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        colliders2D = GetComponentsInChildren<Collider2D>(includeInactive: true);
        colliders3D = GetComponentsInChildren<Collider>(includeInactive: true);
        rb2D = GetComponentInChildren<Rigidbody2D>();
        rb3D = GetComponentInChildren<Rigidbody>();

        // Gom các script điều khiển hay gặp
        movementScripts = GetComponentsInChildren<MonoBehaviour>(true)
            .Where(m => m != null && (
                m.GetType().Name.Contains("Movement") ||
                m.GetType().Name.Contains("Controller") ||
                m.GetType().Name.Contains("Input")
            )).ToArray();

        if (HasStateAuthority)
            RPC_UpdateHealthUI(currentHP, maxHP); // Gửi máu ban đầu
    }

    private void Update()
    {
        // Nút test chết ngay: phím K (chỉ quyền StateAuthority)
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("⚡ TEST DIE pressed");
            TakeDamage(currentHP); // Gây sát thương = máu hiện tại → chết
        }
    }

    // ======= Damage Flow =======
    public void TakeDamage(int amount)
    {
        if (HasStateAuthority)
            ApplyDamage(amount);
        else
            RPC_ApplyDamage(amount);
    }

    public void ApplyDamage(int amount)
    {
        if (!HasStateAuthority) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"💔 Máu còn lại: {currentHP}");

        RPC_UpdateHealthUI(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Debug.Log("💀 Người chơi đã chết");
            RPC_HandleDeath(); // Gọi cho tất cả client để hiển thị thống nhất
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyDamage(int amount) => ApplyDamage(amount);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateHealthUI(int hp, int maxHp)
    {
        currentHP = hp;
        maxHP = maxHp;
        UpdateHealthUI(force: true);
    }

    private void UpdateHealthUI(bool force = false)
    {
        if (healthUI != null)
            healthUI.SetHealth(currentHP, maxHP);
    }

    // ======= Death / Respawn =======
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HandleDeath()
    {
        Debug.Log("🔁 RPC_HandleDeath gọi!");

        // Khoá gameplay ngay khi chết (tắt va chạm/điều khiển, khoá rigidbody)
        SetActiveGameplay(false);

        // Chạy flow: delay 1s → anim Die → chờ gần hết → ẩn → đếm respawn → hồi sinh
        if (animator != null)
            StartCoroutine(IE_DeathFlow());
        else
            StartCoroutine(IE_RespawnOnly());
    }

    private IEnumerator IE_DeathFlow()
    {
        // 1) Đợi 1s mới bật anim chết
        yield return new WaitForSeconds(deathAnimDelay);

        // 2) Bật anim chết (đảm bảo thật sự nhảy vào state Die)
        animator.SetBool("isDead", true);
        animator.CrossFade(dieStateName, 0.05f, dieLayer, 0f);
        yield return null; // chờ 1 frame cho animator cập nhật

        // 3) Chờ tới khi đã vào đúng state "Die"
        var info = animator.GetCurrentAnimatorStateInfo(dieLayer);
        float safety = 0f;
        while (!info.IsName(dieStateName) && safety < 1f) // safety ~1s
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(dieLayer);
            safety += Time.deltaTime;
        }

        // 4) Chờ anim chạy gần hết mới ẩn để không "mất anim die"
        while (info.IsName(dieStateName) && info.normalizedTime < hideAtNormalized)
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(dieLayer);
        }

        // 5) Ẩn player (render) sau khi anim gần xong
        SetVisible(false);

        // 6) Bắt đầu đếm respawn
        if (respawnAfterAnim)
        {
            yield return new WaitForSeconds(respawnDelay);
            DoRespawn();
        }
    }

    // Không có animator vẫn respawn sau delay
    private IEnumerator IE_RespawnOnly()
    {
        yield return new WaitForSeconds(respawnDelay);
        SetVisible(false);
        DoRespawn();
    }

    private void DoRespawn()
    {
        // a) HP & UI: chỉ StateAuthority mới set HP và sync UI
        if (HasStateAuthority)
        {
            currentHP = maxHP;
            RPC_UpdateHealthUI(currentHP, maxHP);
        }

        // b) Đặt lại vị trí: chỉ StateAuthority set transform.position
        if (HasStateAuthority && respawnPoint != null)
            transform.position = respawnPoint.position;

        // c) Reset animator để không kẹt state chết
        ResetAnimator();

        // d) Hiện lại render & bật gameplay
        SetVisible(true);
        SetActiveGameplay(true);

        Debug.Log("🌱 Player đã hồi sinh!");
    }

    private void ResetAnimator()
    {
        if (animator == null) return;

        animator.SetBool("isDead", false);
        animator.ResetTrigger("Die");   // Nếu có dùng trigger
        animator.Rebind();              // Reset toàn bộ state machine về default
        animator.Update(0f);

        // (Tuỳ chọn) đảm bảo về Idle (đổi tên "Idle" nếu khác)
        // animator.Play("Idle", dieLayer, 0f);
        // animator.Update(0f);
    }

    // ======= Helpers: Visible / Gameplay =======
    private void SetVisible(bool visible)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
            if (r) r.enabled = visible;
    }

    private void SetActiveGameplay(bool enable)
    {
        // Bật/tắt collider
        if (colliders2D != null) foreach (var c in colliders2D) if (c) c.enabled = enable;
        if (colliders3D != null) foreach (var c in colliders3D) if (c) c.enabled = enable;

        // Khoá rigidbody để khỏi trôi khi chết
        if (rb2D)
        {
            if (!enable) { rb2D.linearVelocity = Vector2.zero; rb2D.angularVelocity = 0f; }
            rb2D.simulated = enable;
        }
        if (rb3D)
        {
            if (!enable) { rb3D.linearVelocity = Vector3.zero; rb3D.angularVelocity = Vector3.zero; }
            rb3D.isKinematic = !enable; // chỉ bật nếu logic game phù hợp
        }

        // Tắt các script chuyển động/nhập liệu
        if (movementScripts != null)
            foreach (var s in movementScripts) if (s) s.enabled = enable;
    }
}
