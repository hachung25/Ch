using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("Config")]
    public int healAmount = 20;
    public bool consumeOnlyWhenHealed = true; // true: chỉ biến mất khi có hồi máu

    [Header("Feedback (tùy chọn)")]
    public AudioClip pickupSfx;
    public GameObject pickupVFX;

    private bool consumed = false;

    private void Reset()
    {
        // đảm bảo collider là trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        var player = other.GetComponent<PlayerHealth>();
        if (player == null || player.isDead) return;

        int before = player.GetCurrentHealth();
        if (before >= player.MaxHealth)
        {
            // Đang full máu: giữ nguyên currentHealth và KHÔNG tiêu thụ (để lần sau)
            if (!consumeOnlyWhenHealed) TryConsume(player); // nếu bạn muốn vẫn ăn mất dù full
            return;
        }

        // Hồi máu (Heal đã clamp <= MaxHealth)
        player.Heal(healAmount);

        TryConsume(player);
    }

    private void TryConsume(PlayerHealth player)
    {
        consumed = true;

        // VFX
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);

        // SFX: ưu tiên phát qua AudioSource của Player (nếu có)
        if (pickupSfx != null)
        {
            var src = player.GetComponent<AudioSource>();
            if (src != null) src.PlayOneShot(pickupSfx);
            else AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
        }

        Destroy(gameObject);
    }
}
