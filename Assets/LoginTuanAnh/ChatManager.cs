using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;
    private List<string> chatMessages = new List<string>();
    public ChatUI chatUI;

    private void Awake()
    {
        Instance = this;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcReceiveChatMessage(string playerName, string message)
    {
        string formattedMessage = $"{playerName}: {message}";
        chatMessages.Add(formattedMessage);
        chatUI.chatContent.text += formattedMessage + "\n";
    }

    public void SendChatMessage(string message)
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

        RpcReceiveChatMessage(playerName, message);
    }
}
