using UnityEngine;
using System.Collections;

public class TeleportBox : MonoBehaviour
{
    private MapManager mapManager;

    [Header("Unlock Banner (per map)")]
    public TbUnlockMap _Unlock;                 // Banner "Unlock Map"

    [Header("Sound")]
    public AudioClip teleportSound;             // Tiếng NextMap (chỉ phát khi Player BƯỚC VÀO cổng)

    // Lưu mixer group (nếu có) để route cho one-shot
    private UnityEngine.Audio.AudioMixerGroup _mixerGroup;
    private bool _entered = false;              // chống phát 2 lần

    void Awake()
    {
        // Ghi nhớ mixer group nếu bạn có sẵn AudioSource trên object, nhưng KHÔNG để nó auto-play
        var src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        _mixerGroup = src.outputAudioMixerGroup;

        // Tuyệt đối không auto-play trên bất kỳ AudioSource local nào
        SanitizeLocalAudioSources();
    }

    void OnEnable()
    {
        // Khi MapManager bật cổng -> chặn mọi auto-play tiềm ẩn
        SanitizeLocalAudioSources();
        _entered = false; // cho phép đi cổng lại ở map mới
    }

    void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    /// Xóa clip + tắt PlayOnAwake + Stop trên *mọi* AudioSource nằm trên chính TeleportBox (không đụng tới object khác).
    private void SanitizeLocalAudioSources()
    {
        var sources = GetComponents<AudioSource>(); // chỉ trên chính GO này
        foreach (var s in sources)
        {
            if (s == null) continue;
            s.playOnAwake = false;
            s.Stop();
            // Không cho giữ clip sẵn -> tránh PlayOnAwake vô tình bật
            s.clip = null;
        }
    }

    /// Được MapManager gọi sau khi bật cổng: chỉ hiện banner, KHÔNG phát âm
    public void ShowUnlockBannerSafely()
    {
        StartCoroutine(Co_ShowBannerNextFrame());
    }

    private IEnumerator Co_ShowBannerNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        var banner = _Unlock != null ? _Unlock : FindObjectOfType<TbUnlockMap>();
        if (banner != null) banner.showTb();
        else Debug.LogWarning("[TeleportBox] Không tìm thấy TbUnlockMap để hiển thị banner.");
    }

    // COLLISION
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        HandlePlayerEnteredGate();
    }

    // TRIGGER
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HandlePlayerEnteredGate();
    }

    private void HandlePlayerEnteredGate()
    {
        if (_entered) return;
        _entered = true;

        // Phát âm thanh CHỈ khi người chơi vào cổng
        if (teleportSound != null)
            PlayOneShotDetached(teleportSound);

        mapManager?.MoveToNextMap();
    }

    // One-shot tách rời, không bị cắt khi TeleportBox bị tắt ngay sau đó
    private void PlayOneShotDetached(AudioClip clip, float volume = 1f)
    {
        var go = new GameObject("OneShot_Teleport");
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f;
        src.volume = volume;

        if (_mixerGroup != null) src.outputAudioMixerGroup = _mixerGroup;

        src.Play();
        Destroy(go, clip.length + 0.1f);
    }
}
