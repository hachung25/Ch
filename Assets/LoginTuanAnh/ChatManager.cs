using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    private const int maxMessages = 50;
    private List<GameObject> spawnedMessages = new List<GameObject>();

    public ChatUI chatUI;

    [Header("Prefabs")]
    public GameObject listWhitePrefab; // bạn gửi
    public GameObject listBluePrefab;  // người khác gửi

    [Header("Content để chứa tin nhắn")]
    public Transform contentTransform;

    private void Awake()
    {
        Instance = this;
    }

    public void SendChatMessage(string rawMessage)
    {
        string playerName = "Unknown";
        foreach (var player in FindObjectsOfType<PlayerNetwork>())
        {
            if (player.HasInputAuthority)
            {
                playerName = player.PlayerName;
                break;
            }
        }

        // Làm sạch chuỗi, giữ tiếng Việt
        string cleanedMessage = rawMessage.Replace("\r", "").Replace("\n", " ").Trim();

        RpcReceiveChatMessage(playerName, cleanedMessage);
    }

    public void SendSystemMessage(string message)
    {
        RpcReceiveChatMessage("SYSTEM", message, true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcReceiveChatMessage(string playerName, string message, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        bool isMine = false;
        foreach (var player in FindObjectsOfType<PlayerNetwork>())
        {
            if (player.HasInputAuthority && player.PlayerName == playerName)
            {
                isMine = true;
                break;
            }
        }

        GameObject prefabToUse = isMine ? listWhitePrefab : listBluePrefab;

        if (prefabToUse == null || contentTransform == null)
        {
            Debug.LogWarning("❌ Prefab hoặc contentTransform chưa được gán.");
            return;
        }

        GameObject msgObj = Instantiate(prefabToUse, contentTransform);
        ChatMessageUI msgUI = msgObj.GetComponent<ChatMessageUI>();
        if (msgUI != null)
        {
            string displayName = isSystem ? "SYSTEM" : playerName;
            msgUI.Setup(displayName, message);
            msgUI.SetAlignment(isMine); // nếu bạn có căn phải/trái
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
