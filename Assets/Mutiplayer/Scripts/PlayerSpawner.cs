using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [Header("Network Player Prefabs (đã đăng ký trong NetworkProjectConfig)")]
    public NetworkPrefabRef[] playerPrefabs;

    [Header("Spawn Points (tuỳ chọn)")]
    public Transform[] spawnPoints;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawned = new();

    public void PlayerJoined(PlayerRef player)
    {
        // 🔴 CHỈ SERVER/HOST ĐƯỢC SPAWN
        if (!Runner.IsServer)
            return;

        // Chọn prefab: tạm dùng phần tử 0 (nếu có nhiều nhân vật thì xử lý ở PlayerAppearance)
        var prefab = ResolvePrefab(0);

        Vector3 pos = GetSpawnPos(player);

        // Spawn và gán InputAuthority cho player
        var no = Runner.Spawn(prefab, pos, Quaternion.identity, player);
        _spawned[player] = no;
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!Runner.IsServer) return;

        if (_spawned.TryGetValue(player, out var obj) && obj != null)
        {
            Runner.Despawn(obj);
            _spawned.Remove(player);
        }
    }

    // ---------- Helpers ----------
    NetworkPrefabRef ResolvePrefab(int index)
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("[PlayerSpawner] Chưa gán playerPrefabs trong Inspector.");
            return default;
        }
        index = Mathf.Clamp(index, 0, playerPrefabs.Length - 1);
        return playerPrefabs[index];
    }

    Vector3 GetSpawnPos(PlayerRef player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // ❗ KHÔNG ép (int)player → dùng RawEncoded để ra int ổn định
            int i = Mathf.Abs(player.RawEncoded) % spawnPoints.Length;
            return spawnPoints[i].position;
        }
        return new Vector3(0f, 1f, 0f);
    }
}
