using Fusion;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [Header("Network Player Prefabs (đã đăng ký trong NetworkProjectConfig)")]
    public NetworkPrefabRef[] playerPrefabs;

    [Header("Spawn Points (tuỳ chọn)")]
    public Transform[] spawnPoints;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawned = new();
    private Coroutine _catchUpCo;

    void OnEnable() => _catchUpCo = StartCoroutine(CatchUpSpawnWhenRunnerReady());
    void OnDisable() { if (_catchUpCo != null) StopCoroutine(_catchUpCo); _spawned.Clear(); }

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
            _spawned.Remove(player);
        }
        else if (_spawned.TryGetValue(player, out var obj) && obj)
        {
            Runner.Despawn(obj);
            _spawned.Remove(player);
        }
    }

    void EnsureSpawnFor(NetworkRunner runner, PlayerRef player)
    {
        if (runner.TryGetPlayerObject(player, out var existing) && existing)
        {
            _spawned[player] = existing;
            return;
        }
        if (_spawned.TryGetValue(player, out var already) && already) return;

        // Mỗi client lưu nhân vật riêng trong PlayerPrefs
        int prefabIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        var prefab = ResolvePrefab(prefabIndex);
        if (!prefab.IsValid) { Debug.LogError("[PlayerSpawner] PrefabRef invalid."); return; }

        var obj = runner.Spawn(prefab, GetSpawnPos(player), Quaternion.identity, player);

        if (!runner.TryGetPlayerObject(player, out _))
            runner.SetPlayerObject(player, obj);

        _spawned[player] = obj;
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

    Vector3 GetSpawnPos(PlayerRef player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int i = Mathf.Abs(player.RawEncoded) % spawnPoints.Length;
            var t = spawnPoints[i];
            if (t) return t.position;
        }
        return new Vector3(0f, 1f, 0f);
    }
}