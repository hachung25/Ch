using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Vector3 spawnPos = new Vector3(player.PlayerId * 2, 1, 0); 
            Runner.Spawn(PlayerPrefab, spawnPos, Quaternion.identity, player);
        }
    }
}
