using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    private const int maxMessages = 50;
    private readonly List<GameObject> spawnedMessages = new();

    public ChatUI chatUI;

    [Header("Prefabs")]
    public GameObject listWhitePrefab; // tin của tôi
    public GameObject listBluePrefab;  // tin của người khác

    [Header("Content để chứa tin nhắn")]
    public Transform contentTransform;

    void Awake() { Instance = this; }

    // ===== GỬI =====
    public void SendChatMessage(string rawMessage)
    {
        string cleaned = (rawMessage ?? "").Replace("\r", "").Replace("\n", " ").Trim();
        if (string.IsNullOrWhiteSpace(cleaned)) return;

        var runner = Runner ? Runner : FindObjectOfType<NetworkRunner>();
        if (runner == null) { Debug.LogWarning("[Chat] Runner chưa sẵn sàng."); return; }

        if (!runner.TryGetPlayerObject(runner.LocalPlayer, out var pObj) || pObj == null)
        { Debug.LogWarning("[Chat] Local PlayerObject chưa sẵn sàng."); return; }

        // Lấy tên từ NicknameSync (networked) – không dùng PlayerName.Current
        var nick = pObj.GetComponent<NicknameSync>();
        string displayName = nick ? nick.Nickname.ToString() : $"Player#{runner.LocalPlayer.RawEncoded}";

        // gửi kèm id người gửi (RawEncoded) để client tự xác định isMine
        RpcReceiveChatMessage(displayName, cleaned, runner.LocalPlayer.RawEncoded, false);
    }

    public void SendSystemMessage(string message)
    {
        RpcReceiveChatMessage("SYSTEM", message ?? "", int.MinValue, true);
    }

    // ===== NHẬN (mọi client) =====
    [Rpc(RpcSources.All, RpcTargets.All)]
    void RpcReceiveChatMessage(string fromName, string message, int senderRaw, bool isSystem)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        bool isMine = !isSystem && Runner && Runner.LocalPlayer.RawEncoded == senderRaw;

        var prefab = isMine ? listWhitePrefab : listBluePrefab;
        if (prefab == null || contentTransform == null)
        {
            Debug.LogWarning("[Chat] Prefab hoặc contentTransform chưa gán.");
            return;
        }

        var msgObj = Instantiate(prefab, contentTransform);
        var ui = msgObj.GetComponent<ChatMessageUI>();
        if (ui != null)
        {
            ui.Setup(isSystem ? "SYSTEM" : fromName, message);
            ui.SetAlignment(isMine); // căn phải/trái nếu bạn có
            spawnedMessages.Add(msgObj);
        }

        if (spawnedMessages.Count > maxMessages)
        {
            Destroy(spawnedMessages[0]);
            spawnedMessages.RemoveAt(0);
        }

        if (chatUI != null && chatUI.scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatUI.scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
