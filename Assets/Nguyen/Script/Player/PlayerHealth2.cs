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

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 3f;

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

    // ===== Death / Respawn Flow (giữ nguyên logic gốc) =====
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

        if (HasStateAuthority && respawnPoint != null)
            transform.position = respawnPoint.position;

        ResetAnimator();

        SetVisible(true);
        SetHealthUIVisible(true);
        SetActiveGameplay(true);

        Debug.Log("🌱 Player đã hồi sinh!");
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
