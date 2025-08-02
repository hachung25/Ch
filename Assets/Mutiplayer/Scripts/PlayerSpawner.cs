using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    // Tạo một mảng các prefab nhân vật
    public GameObject[] PlayerPrefabs;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            GameObject playerPrefabToSpawn = null;

            // Lấy chỉ số nhân vật đã chọn từ CharacterSelectionManager
            if (CharacterSelectionManager.Instance != null && CharacterSelectionManager.Instance.selectedCharacterIndex != -1)
            {
                int selectedIndex = CharacterSelectionManager.Instance.selectedCharacterIndex;

                // Lấy prefab tương ứng từ mảng
                if (selectedIndex < PlayerPrefabs.Length)
                {
                    playerPrefabToSpawn = PlayerPrefabs[selectedIndex];
                }
            }

            // Nếu không có prefab nào được chọn, dùng prefab mặc định
            if (playerPrefabToSpawn == null)
            {
                // Tùy chọn: dùng prefab đầu tiên trong mảng làm mặc định
                playerPrefabToSpawn = PlayerPrefabs[0];
            }

            // Sinh ra nhân vật
            NetworkObject playerObject = Runner.Spawn(playerPrefabToSpawn, new Vector3(0, 1, 0), Quaternion.identity, player);

            // Gán transform của nhân vật vừa sinh ra vào target của CameraFollow
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.target = playerObject.transform;
            }
        }
    }
}