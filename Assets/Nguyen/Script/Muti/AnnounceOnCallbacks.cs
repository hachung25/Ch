using System;
using System.Collections.Generic;
using Firebase.Auth;
using Fusion;
using Fusion.Sockets;            // <- cần cho NetAddress, NetConnectFailedReason, NetDisconnectReason, ReliableKey
using UnityEngine;

public class AnnounceOnCallbacks : MonoBehaviour, INetworkRunnerCallbacks
{
    void OnEnable()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null) runner.AddCallbacks(this);
    }

    void OnDisable()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null) runner.RemoveCallbacks(this);
    }

    string GetUid() => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
    int GetCharIndex() => PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

    // ====== chính: gọi RPC báo danh khi client local vừa join ======
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer) return; // chỉ báo danh cho chính mình
        var announcer = FindObjectOfType<JoinAnnounce>();
        if (announcer)
        {
            var uid = GetUid();
            var ci = GetCharIndex();
            Debug.Log($"[AnnounceOnCallbacks] Announcing uid={uid}, char={ci}");
            announcer.RPC_Announce(uid, ci);
        }
        else
        {
            Debug.LogError("[AnnounceOnCallbacks] Không tìm thấy JoinAnnounce. Hãy chắc chắn 'Announcer' là Scene Object & có NetworkObject và đã add trong NetworkProjectConfig → Scene Objects.");
        }
    }

    // ====== các callback còn lại để trống theo đúng chữ ký Fusion 2.0 ======
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }                    // KHÔNG dùng ref trong Fusion 2.0
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
