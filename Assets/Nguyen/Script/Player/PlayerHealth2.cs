using UnityEngine;
using Fusion;
using System.Collections;
using System.Linq;

public class PlayerHealth2 : NetworkBehaviour, IDamageable
{
    [SerializeField] private int currentHP;
    public int maxHP = 100;

    private PlayerHealthUI2 healthUI;
    private Animator animator;
    [Header("Health UI Root (optional)")]
    public GameObject healthUIRoot;

    // ===== Respawn Options =====
    [Header("Respawn Settings")]
    public float respawnDelay = 3f;

    public enum RespawnMode { FixedPoint, RandomPoints, RandomArea, RoundRobin }

    [Header("Respawn Options")]
    public RespawnMode respawnSelectMode = RespawnMode.FixedPoint;

    [Tooltip("Dùng cho FixedPoint (1 điểm cố định)")]
    public Transform respawnPoint;                 // fallback / fixed point

    [Tooltip("Dùng cho RandomPoints / RoundRobin")]
    public Transform[] respawnPoints;              // danh sách nhiều điểm

    [Tooltip("Dùng cho RandomArea: tâm khu vực")]
    public Transform respawnAreaCenter;            // tâm khu vực

    [Tooltip("Dùng cho RandomArea: bán kính khu vực")]
    public float respawnAreaRadius = 8f;           // bán kính

    [Networked] private int rrIndex { get; set; }  // RoundRobin index

    // ===== Spawn Safety Check =====
    [Header("Spawn Safety (tránh trùng va chạm)")]
    [Tooltip("Chọn TRUE nếu game 2D, FALSE nếu 3D")]
    public bool use2DPhysics = true;

    [Tooltip("Số lần thử random vị trí (khi RandomArea) hoặc duyệt điểm (khi Points)")]
    public int maxSpawnTries = 12;

    [Tooltip("Bán kính kiểm tra vị trí trống (2D)")]
    public float spawnCheckRadius2D = 0.5f;

    [Tooltip("Layer nào coi là cản trở khi spawn (2D)")]
    public LayerMask spawnBlockMask2D = ~0; // mặc định: mọi layer đều block

    [Tooltip("Bán kính kiểm tra vị trí trống (3D)")]
    public float spawnCheckRadius3D = 0.5f;

    [Tooltip("Layer nào coi là cản trở khi spawn (3D)")]
    public LayerMask spawnBlockMask3D = ~0;

    [Header("Death Animation")]
    public string dieStateName = "Die";
    public int dieLayer = 0;
    [Range(0f, 1f)] public float hideAtNormalized = 0.95f;
    public bool respawnAfterAnim = true;
    public float deathAnimStateTimeout = 2f;
    public float deathAnimFinishTimeout = 5f;

    private Renderer[] renderers;
    private Collider2D[] colliders2D;
    private Collider[] colliders3D;
    private Rigidbody2D rb2D;
    private Rigidbody rb3D;
    private MonoBehaviour[] movementScripts;

    // NEW: attacker
    private PlayerStats lastAttacker;

    public override void Spawned()
    {
        if (HasStateAuthority)
            currentHP = maxHP;

        healthUI = GetComponentInChildren<PlayerHealthUI2>(true);
        animator = GetComponentInChildren<Animator>();

        if (healthUIRoot == null && healthUI != null)
            healthUIRoot = healthUI.gameObject;

        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        colliders2D = GetComponentsInChildren<Collider2D>(includeInactive: true);
        colliders3D = GetComponentsInChildren<Collider>(includeInactive: true);
        rb2D = GetComponentInChildren<Rigidbody2D>();
        rb3D = GetComponentInChildren<Rigidbody>();

        movementScripts = GetComponentsInChildren<MonoBehaviour>(true)
            .Where(m => m != null && (
                m.GetType().Name.Contains("Movement") ||
                m.GetType().Name.Contains("Controller") ||
                m.GetType().Name.Contains("Input")
            )).ToArray();

        if (HasStateAuthority)
            RPC_UpdateHealthUI(currentHP, maxHP);
    }

    private void Update()
    {
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("⚡ TEST DIE pressed");
            TakeDamage(currentHP);
        }
    }

    // ===== Damage Flow =====
    public void TakeDamage(int amount)
    {
        if (HasStateAuthority)
            ApplyDamage(amount);
        else
            RPC_ApplyDamage(amount);
    }

    public void TakeDamageFrom(PlayerStats attacker, int amount)
    {
        if (HasStateAuthority)
        {
            lastAttacker = attacker;
            ApplyDamage(amount);
        }
        else
        {
            RPC_TakeDamageFrom(attacker.Object.InputAuthority, amount);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_TakeDamageFrom(PlayerRef attackerRef, int amount)
    {
        var attackerObj = Runner.GetPlayerObject(attackerRef);
        if (attackerObj != null && attackerObj.TryGetComponent(out PlayerStats stats))
        {
            lastAttacker = stats;
        }
        ApplyDamage(amount);
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

            // ✅ Ghi nhận Kill/Death
            if (lastAttacker != null)
                lastAttacker.AddKill();

            var myStats = GetComponent<PlayerStats>();
            myStats?.AddDeath();

            RPC_HandleDeath();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyDamage(int amount) => ApplyDamage(amount);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateHealthUI(int hp, int maxHp)
    {
        currentHP = hp;
        maxHP = maxHp;
        if (healthUI != null)
            healthUI.SetHealth(currentHP, maxHP);
    }

    // ===== Death / Respawn Flow =====
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HandleDeath()
    {
        Debug.Log("🔁 RPC_HandleDeath gọi!");
        SetActiveGameplay(false);

        if (animator != null)
            StartCoroutine(IE_DeathFlow());
        else
            StartCoroutine(IE_RespawnOnly());
    }

    private IEnumerator IE_DeathFlow()
    {
        animator.SetBool("isDead", true);
        animator.CrossFade(dieStateName, 0.05f, dieLayer, 0f);
        yield return null;

        float t = 0f;
        var info = animator.GetCurrentAnimatorStateInfo(dieLayer);
        while (!info.IsName(dieStateName) && t < deathAnimStateTimeout)
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(dieLayer);
            t += Time.deltaTime;
        }

        t = 0f;
        while (info.IsName(dieStateName) && info.normalizedTime < hideAtNormalized && t < deathAnimFinishTimeout)
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(dieLayer);
            t += Time.deltaTime;
        }

        SetVisible(false);
        SetHealthUIVisible(false);

        if (respawnAfterAnim)
        {
            yield return new WaitForSeconds(respawnDelay);
            DoRespawn();
        }
    }

    private IEnumerator IE_RespawnOnly()
    {
        SetHealthUIVisible(false);
        yield return new WaitForSeconds(respawnDelay);
        SetVisible(false);
        DoRespawn();
    }

    private void DoRespawn()
    {
        if (HasStateAuthority)
        {
            currentHP = maxHP;
            RPC_UpdateHealthUI(currentHP, maxHP);
        }

        if (HasStateAuthority)
        {
            // Server chọn điểm respawn và đặt vị trí
            Vector3 spawnPos = GetRespawnPositionSafe();
            transform.position = spawnPos;
        }

        ResetAnimator();

        SetVisible(true);
        SetHealthUIVisible(true);
        SetActiveGameplay(true);

        Debug.Log("🌱 Player đã hồi sinh!");
    }

    // === Chọn vị trí respawn với kiểm tra an toàn ===
    private Vector3 GetRespawnPositionSafe()
    {
        // Chọn candidate theo mode
        switch (respawnSelectMode)
        {
            case RespawnMode.RandomArea:
                return FindSafeInArea();

            case RespawnMode.RoundRobin:
                {
                    Vector3? pos = FindSafeInPoints(roundRobin: true);
                    if (pos.HasValue) return pos.Value;
                    // fallback: thử area nếu có
                    if (respawnAreaCenter) return FindSafeInArea();
                    break;
                }

            case RespawnMode.RandomPoints:
                {
                    Vector3? pos = FindSafeInPoints(roundRobin: false);
                    if (pos.HasValue) return pos.Value;
                    if (respawnAreaCenter) return FindSafeInArea();
                    break;
                }

            case RespawnMode.FixedPoint:
            default:
                {
                    if (respawnPoint != null)
                    {
                        Vector3 p = respawnPoint.position;
                        return Ensure2DZ(p);
                    }
                    break;
                }
        }

        // Fallback cuối: giữ nguyên vị trí hiện tại
        return Ensure2DZ(transform.position);
    }

    private Vector3? FindSafeInPoints(bool roundRobin)
    {
        if (respawnPoints == null || respawnPoints.Length == 0) return null;

        int len = respawnPoints.Length;
        int tries = Mathf.Max(1, Mathf.Min(maxSpawnTries, len));

        if (roundRobin)
        {
            for (int i = 0; i < tries; i++)
            {
                int index = (rrIndex + i) % len;
                Transform t = respawnPoints[index];
                if (t == null) continue;

                Vector3 candidate = Ensure2DZ(t.position);
                if (IsPositionFree(candidate))
                {
                    rrIndex = (index + 1) % len; // advance
                    return candidate;
                }
            }
            // nếu không chỗ nào trống thì vẫn tăng index để không kẹt
            rrIndex = (rrIndex + 1) % len;
        }
        else
        {
            for (int i = 0; i < tries; i++)
            {
                int idx = Random.Range(0, len);
                Transform t = respawnPoints[idx];
                if (t == null) continue;

                Vector3 candidate = Ensure2DZ(t.position);
                if (IsPositionFree(candidate))
                    return candidate;
            }
        }

        return null;
    }

    private Vector3 FindSafeInArea()
    {
        Vector3 fallback = respawnAreaCenter ? Ensure2DZ(respawnAreaCenter.position) : Ensure2DZ(transform.position);

        if (respawnAreaCenter == null || respawnAreaRadius <= 0f)
            return fallback;

        for (int i = 0; i < maxSpawnTries; i++)
        {
            Vector2 rand = Random.insideUnitCircle * respawnAreaRadius;
            Vector3 candidate = respawnAreaCenter.position + new Vector3(rand.x, 0f, rand.y);
            candidate = Ensure2DZ(candidate);

            if (IsPositionFree(candidate))
                return candidate;
        }

        // nếu thử nhiều lần vẫn không có chỗ, dùng fallback
        return fallback;
    }

    // Giữ nguyên Z (2D) hoặc giữ nguyên Y (tuỳ game) – ở đây:
    // - 2D: ép Z về Z hiện tại của player
    // - 3D: trả nguyên candidate
    private Vector3 Ensure2DZ(Vector3 candidate)
    {
        if (use2DPhysics)
            return new Vector3(candidate.x, candidate.y, transform.position.z);
        return candidate;
    }

    private bool IsPositionFree(Vector3 pos)
    {
        if (use2DPhysics)
        {
            // Không đè lên collider khác
            var hit = Physics2D.OverlapCircle((Vector2)pos, spawnCheckRadius2D, spawnBlockMask2D);
            return hit == null;
        }
        else
        {
            // 3D
            bool blocked = Physics.CheckSphere(pos, spawnCheckRadius3D, spawnBlockMask3D, QueryTriggerInteraction.Ignore);
            return !blocked;
        }
    }

    private void ResetAnimator()
    {
        if (animator == null) return;

        animator.SetBool("isDead", false);
        animator.ResetTrigger("Die");
        animator.Rebind();
        animator.Update(0f);
    }

    // ===== Helpers =====
    private void SetVisible(bool visible)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
            if (r) r.enabled = visible;
    }

    private void SetHealthUIVisible(bool visible)
    {
        if (healthUIRoot != null)
            healthUIRoot.SetActive(visible);
        else if (healthUI != null && healthUI.gameObject != null)
            healthUI.gameObject.SetActive(visible);
    }

    private void SetActiveGameplay(bool enable)
    {
        if (colliders2D != null) foreach (var c in colliders2D) if (c) c.enabled = enable;
        if (colliders3D != null) foreach (var c in colliders3D) if (c) c.enabled = enable;

        if (rb2D)
        {
            if (!enable) { rb2D.linearVelocity = Vector2.zero; rb2D.angularVelocity = 0f; }
            rb2D.simulated = enable;
        }
        if (rb3D)
        {
            if (!enable) { rb3D.linearVelocity = Vector3.zero; rb3D.angularVelocity = Vector3.zero; }
            rb3D.isKinematic = !enable;
        }

        if (movementScripts != null)
            foreach (var s in movementScripts) if (s) s.enabled = enable;
    }
}
