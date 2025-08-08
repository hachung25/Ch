using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using System.Collections.Generic; // Cho List<> và Dictionary<>
using UnityEngine.SceneManagement; // Cho SceneManager
using System; // Cho ArraySegment<>


public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public InputField roomNameInput;
    public Button createRoomButton;
    public RoomListUI roomListUI;

    private NetworkRunner runner;

    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private GameObject lobbyPanel;



    private async void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "Lobby",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        runner.AddCallbacks(this);

        createRoomButton.onClick.AddListener(() =>
        {
            string roomName = roomNameInput.text;
            CreateRoom(roomName);
        });
    }

    public async void CreateRoom(string roomName)
    {
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });

        if (result.Ok)
        {
            Debug.Log("Room created successfully!");
            lobbyPanel.SetActive(false); // ✅ Ẩn panel lobby sau khi tạo phòng
        }
        else
        {
            Debug.LogError("Failed to create room: " + result.ShutdownReason);
        }
    }


    public async void JoinRoom(string roomName)
    {
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });

        if (result.Ok)
        {
            Debug.Log("Joined room successfully!");
            lobbyPanel.SetActive(false); // ✅ Ẩn panel lobby sau khi vào phòng
        }
        else
        {
            Debug.LogError("Failed to join room: " + result.ShutdownReason);
        }
    }


    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        roomListUI.UpdateRoomList(sessionList);
    }

    // Bỏ qua các hàm callback khác nếu không cần xử lý
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 1, 0);
            runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }



}
