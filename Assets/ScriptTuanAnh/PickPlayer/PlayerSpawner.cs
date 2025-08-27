using Fusion;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [Header("Network Player Prefabs (chỉ lấy prefab đầu tiên)")]
    public NetworkPrefabRef[] playerPrefabs;

    [Header("Spawn Points (tuỳ chọn)")]
    public Transform[] spawnPoints;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawned = new();

    void Start()
    {
        // Khi Runner đã sẵn sàng -> catch-up toàn bộ player đang có
        if (IsServerOrMaster() && Runner != null)
        {
            Debug.Log("[PlayerSpawner] Catch-up ActivePlayers khi Start()");
            foreach (var p in Runner.ActivePlayers)
            {
                EnsureSpawnFor(Runner, p);
            }
        }
    }

    // Khi player mới join
    public void PlayerJoined(PlayerRef player)
    {
        if (!IsServerOrMaster()) return;

        Debug.Log($"[PlayerSpawner] PlayerJoined: {player}");
        EnsureSpawnFor(Runner, player);
    }

    // Khi player rời đi
    public void PlayerLeft(PlayerRef player)
    {
        if (!IsServerOrMaster()) return;

        Debug.Log($"[PlayerSpawner] PlayerLeft: {player}");

        if (Runner.TryGetPlayerObject(player, out var pObj) && pObj)
        {
            Runner.Despawn(pObj);
            _spawned.Remove(player);
        }
        else if (_spawned.TryGetValue(player, out var obj) && obj)
        {
            Runner.Despawn(obj);
            _spawned.Remove(player);
        }
    }

    // Hàm spawn an toàn
    private void EnsureSpawnFor(NetworkRunner runner, PlayerRef player)
    {
        if (runner.TryGetPlayerObject(player, out var existing) && existing)
        {
            Debug.Log($"[PlayerSpawner] Player {player} đã có object: {existing.name}");
            _spawned[player] = existing;
            return;
        }

        if (_spawned.TryGetValue(player, out var already) && already)
        {
            Debug.Log($"[PlayerSpawner] Player {player} đã tồn tại trong _spawned.");
            return;
        }

        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("[PlayerSpawner] ❌ Thiếu playerPrefabs trong inspector.");
            return;
        }

        // ✅ Luôn spawn prefab đầu tiên
        var prefab = playerPrefabs[0];
        var pos = GetSpawnPos(player);

        Debug.Log($"[PlayerSpawner] Đang spawn prefab {prefab} cho player {player} tại {pos}");
        var obj = runner.Spawn(prefab, pos, Quaternion.identity, player);

        if (!runner.TryGetPlayerObject(player, out _))
        {
            Debug.Log($"[PlayerSpawner] Gán SetPlayerObject cho {player}");
            runner.SetPlayerObject(player, obj);
        }

        _spawned[player] = obj;
        Debug.Log($"[PlayerSpawner] ✅ Spawn thành công cho player {player}");
    }

    // Lấy vị trí spawn
    private Vector3 GetSpawnPos(PlayerRef player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int i = Mathf.Abs(player.RawEncoded) % spawnPoints.Length;
            var t = spawnPoints[i];
            if (t) return t.position;
        }
        return new Vector3(0f, 1f, 0f);
    }

    // Kiểm tra quyền server/master
    private bool IsServerOrMaster()
    {
        return Runner != null && (Runner.IsServer || Runner.IsSharedModeMasterClient);
    }
}
