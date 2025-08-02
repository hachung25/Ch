using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            // Sinh ra nhân vật và lưu trữ NetworkObject được trả về
            NetworkObject playerObject = Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);

            // Gán transform của nhân vật vừa sinh ra vào target của CameraFollow
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.target = playerObject.transform;
            }
        }
    }
}