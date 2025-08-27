using UnityEngine;
using System.Collections;

public class TeleportBox : MonoBehaviour
{
    private MapManager mapManager;

    [Header("Unlock Banner (per map)")]
    public TbUnlockMap _Unlock;           // Kéo object UI có TbUnlockMap (banner "Unlock Map")

    [Header("Sound")]
    public AudioClip teleportSound;       // Tiếng NextMap
    private AudioSource audioSource;      // chỉ để giữ routing mixer nếu bạn dùng

    void Awake()
    {
        // Chuẩn bị AudioSource (để giữ output mixer group nếu có)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
    }

    void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    /// <summary>
    /// Được MapManager gọi NGAY SAU khi bật teleportBox (sau khi clear map).
    /// Chờ 1 frame để UI/Canvas sẵn sàng rồi show banner.
    /// </summary>
    public void ShowUnlockBannerSafely()
    {
        StartCoroutine(Co_ShowBannerNextFrame());
    }

    private IEnumerator Co_ShowBannerNextFrame()
    {
        yield return null;                   // chờ 1 frame
        yield return new WaitForEndOfFrame();// đảm bảo UI đã vẽ xong

        var banner = _Unlock != null ? _Unlock : FindObjectOfType<TbUnlockMap>();
        if (banner != null) banner.showTb();
        else Debug.LogWarning("[TeleportBox] Không tìm thấy TbUnlockMap để hiển thị banner.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        // PHÁT ÂM THANH TÁCH RỜI -> không bị cắt khi MoveToNextMap() tắt teleporter
        //if (teleportSound != null)
        //    PlayOneShotDetached(teleportSound);

        mapManager?.MoveToNextMap();
    }

    // Nếu teleporter dùng trigger thay vì collision:
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        //if (teleportSound != null)
        //    PlayOneShotDetached(teleportSound);

        mapManager?.MoveToNextMap();
    }

    // Tạo 1 AudioSource rời, tự hủy sau khi phát xong
    private void PlayOneShotDetached(AudioClip clip, float volume = 1f)
    {
        var go = new GameObject("OneShot_Teleport");
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D
        src.volume = volume;

        // Giữ routing AudioMixer nếu có
        if (audioSource != null && audioSource.outputAudioMixerGroup != null)
            src.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;

        src.Play();
        Destroy(go, clip.length + 0.1f);
    }
}
