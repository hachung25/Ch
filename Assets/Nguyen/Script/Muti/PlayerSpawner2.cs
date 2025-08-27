using Fusion;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public class PlayerSpawner2 : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [Header("Network Player Prefabs (đã đăng ký trong NetworkProjectConfig)")]
    public NetworkPrefabRef[] playerPrefabs;

    [Header("Spawn Points (tuỳ chọn)")]
    public Transform[] spawnPoints;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawned = new();

    // === NEW: quản lý slot spawn đang dùng ===
    private readonly HashSet<int> _usedSpawnIndices = new();
    private readonly Dictionary<PlayerRef, int> _playerSpawnIndex = new();

    private Coroutine _catchUpCo;

    void OnEnable() => _catchUpCo = StartCoroutine(CatchUpSpawnWhenRunnerReady());
    void OnDisable() { if (_catchUpCo != null) StopCoroutine(_catchUpCo); _spawned.Clear(); _usedSpawnIndices.Clear(); _playerSpawnIndex.Clear(); }

    IEnumerator CatchUpSpawnWhenRunnerReady()
    {
        NetworkRunner runner = null;
        while ((runner = FindObjectOfType<NetworkRunner>()) == null) yield return null;
        yield return null; // 1 frame

        if (!runner.IsServer) yield break;

        foreach (var p in runner.ActivePlayers)
            EnsureSpawnFor(runner, p);
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer) return;
        EnsureSpawnFor(Runner, player);
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!Runner.IsServer) return;

        if (Runner.TryGetPlayerObject(player, out var pObj) && pObj)
        {
            Runner.Despawn(pObj);
        }
        _spawned.Remove(player);

        // === NEW: giải phóng slot spawn đã giữ cho player này
        if (_playerSpawnIndex.TryGetValue(player, out var idx))
        {
            _usedSpawnIndices.Remove(idx);
            _playerSpawnIndex.Remove(player);
        }

        FusionIdentityBridge.Clear(player); // dọn map
    }

    void EnsureSpawnFor(NetworkRunner runner, PlayerRef player)
    {
        if (runner.TryGetPlayerObject(player, out var existing) && existing)
        {
            _spawned[player] = existing;
            return;
        }
        if (_spawned.TryGetValue(player, out var already) && already) return;

        StartCoroutine(SpawnWhenIdentityReady(runner, player));
    }

    IEnumerator SpawnWhenIdentityReady(NetworkRunner runner, PlayerRef player)
    {
        float t = 0f;
        while (!FusionIdentityBridge.PlayerToFirebaseUid.ContainsKey(player) && t < 3f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        int prefabIndex = 0;

        if (FusionIdentityBridge.PlayerToFirebaseUid.TryGetValue(player, out var firebaseUid))
        {
            var dict = RoomService.I?.GetPlayersSnapshot();
            if (dict != null && dict.TryGetValue(firebaseUid, out var info))
            {
                prefabIndex = info.characterIndex;
            }
            else if (FusionIdentityBridge.PlayerToCharIndex.TryGetValue(player, out var ci))
            {
                prefabIndex = ci;
            }
            else
            {
                prefabIndex = 0;
                Debug.LogWarning($"[PlayerSpawner] No cache for {firebaseUid}, default 0");
            }
        }
        else if (FusionIdentityBridge.PlayerToCharIndex.TryGetValue(player, out var ci2))
        {
            prefabIndex = ci2;
        }
        else
        {
            prefabIndex = 0;
            Debug.LogWarning("[PlayerSpawner] Identity not ready, default 0");
        }

        var prefab = ResolvePrefab(prefabIndex);
        if (!prefab.IsValid) { Debug.LogError("[PlayerSpawner] PrefabRef invalid."); yield break; }

        // === NEW: nếu đây là respawn, giải phóng slot cũ để random lại
        if (_playerSpawnIndex.TryGetValue(player, out var prevIdx))
        {
            _usedSpawnIndices.Remove(prevIdx);
            _playerSpawnIndex.Remove(player);
        }

        var spawnPos = GetRandomSpawnPosAndReserve(player);

        var obj = runner.Spawn(prefab, spawnPos, Quaternion.identity, player);

        if (!runner.TryGetPlayerObject(player, out _))
            runner.SetPlayerObject(player, obj);

        _spawned[player] = obj;
        Debug.Log($"[PlayerSpawner] Spawned {player} with prefab index {prefabIndex}");
    }

    NetworkPrefabRef ResolvePrefab(int index)
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("[PlayerSpawner] Thiếu playerPrefabs.");
            return default;
        }
        index = Mathf.Clamp(index, 0, playerPrefabs.Length - 1);
        return playerPrefabs[index];
    }

    // === NEW: random mỗi lần spawn, ưu tiên slot chưa ai dùng
    Vector3 GetRandomSpawnPosAndReserve(PlayerRef player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int n = spawnPoints.Length;

            // gom các index đang rảnh
            List<int> free = null;
            if (_usedSpawnIndices.Count < n)
            {
                free = new List<int>(n - _usedSpawnIndices.Count);
                for (int i = 0; i < n; i++)
                {
                    if (!_usedSpawnIndices.Contains(i)) free.Add(i);
                }

                int idx = free[Random.Range(0, free.Count)];
                _usedSpawnIndices.Add(idx);
                _playerSpawnIndex[player] = idx;

                var t = spawnPoints[idx];
                if (t) return t.position;
            }
            else
            {
                // tất cả đều đang dùng → cho phép chọn trùng
                int idx = Random.Range(0, n);
                _playerSpawnIndex[player] = idx;

                var t = spawnPoints[idx];
                if (t) return t.position;
            }
        }

        // fallback khi không có spawnPoints
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }
}
