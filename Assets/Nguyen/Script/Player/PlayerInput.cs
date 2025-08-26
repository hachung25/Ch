using UnityEngine;
using Fusion;
using Fusion.Sockets;

// Struct input cho Fusion
public struct PlayerInput : INetworkInput
{
    public float horizontal;
    public NetworkBool jump;
    public NetworkBool attack;
}

public class InputProvider : MonoBehaviour, INetworkRunnerCallbacks
{
    private void Awake()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
    }

    // Fusion sẽ gọi hàm này mỗi tick để lấy input
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (ChatState.IsChatting)
            return;

        PlayerInput data = new PlayerInput
        {
            horizontal = Input.GetAxisRaw("Horizontal"),
            jump = Input.GetKeyDown(KeyCode.Y),
            attack = Input.GetKey(KeyCode.T)
        };

        input.Set(data);
    }

    // Callback input missing (bắt buộc trong Fusion 2.0)
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    // Các callback khác (để trống cho compile)
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
}
