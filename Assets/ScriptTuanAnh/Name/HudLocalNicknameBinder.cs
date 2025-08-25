using System.Collections;
using TMPro;
using UnityEngine;
using Fusion;

[DisallowMultipleComponent]
public class HudLocalNicknameBinder : MonoBehaviour
{
    [Tooltip("TMP_Text hiển thị tên ở góc trái")]
    public TMP_Text label;

    [Tooltip("Tiền tố hiển thị, ví dụ \"\" hoặc \"User: \".")]
    public string prefix = "";

    private NicknameSync _boundNick;

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        StartCoroutine(RebindCoroutine());
    }

    void OnDisable()
    {
        if (_boundNick != null)
        {
            _boundNick.NicknameChanged -= OnNicknameChanged;
            _boundNick = null;
        }
    }

    IEnumerator RebindCoroutine()
    {
        // 1) Chờ Runner
        NetworkRunner runner = null;
        while ((runner = FindObjectOfType<NetworkRunner>()) == null)
            yield return null;

        // 2) Chờ PlayerObject của LocalPlayer
        NetworkObject po = null;
        while (!runner.TryGetPlayerObject(runner.LocalPlayer, out po) || po == null)
            yield return null;

        // 3) Lấy NicknameSync và bind sự kiện
        var nick = po.GetComponent<NicknameSync>();
        _boundNick = nick;

        string initial = nick ? nick.Nickname.ToString() : $"Player#{runner.LocalPlayer.RawEncoded}";
        if (label) label.SetText(prefix + initial);

        if (nick != null)
            nick.NicknameChanged += OnNicknameChanged;
    }

    void OnNicknameChanged(string newName)
    {
        if (label) label.SetText(prefix + newName);
    }

    // Dành cho debug nhanh trong Inspector
    [ContextMenu("Force Rebind")]
    void ForceRebind()
    {
        OnDisable();
        StopAllCoroutines();
        StartCoroutine(RebindCoroutine());
    }
}
