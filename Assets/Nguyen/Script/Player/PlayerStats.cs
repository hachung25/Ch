using System.Linq;
using Fusion;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [Networked] public int Kills { get; private set; }
    [Networked] public int Deaths { get; private set; }

    public static event System.Action OnAnyStatsChanged;

    public void AddKill()
    {
        if (!HasStateAuthority) return;
        Kills++;
        OnAnyStatsChanged?.Invoke();
        Debug.Log($"🔥 Player {Object.InputAuthority} có {Kills} kills");
    }

    public void AddDeath()
    {
        if (!HasStateAuthority) return;
        Deaths++;
        OnAnyStatsChanged?.Invoke();
        Debug.Log($"☠️ Player {Object.InputAuthority} đã chết {Deaths} lần");
    }

    // 👉 Hàm trả về bảng xếp hạng (đã sort)
    public static PlayerStats[] GetSortedPlayers()
    {
        var all = FindObjectsOfType<PlayerStats>();
        return all
            .OrderByDescending(p => p.Kills)
            .ThenBy(p => p.Deaths)
            .ToArray();
    }

    // 👉 Gọi ở host để trao thưởng cho tất cả player
    public static void DistributeRewards()
    {
        var sorted = GetSortedPlayers();

        for (int i = 0; i < sorted.Length; i++)
        {
            int rank = i + 1;
            sorted[i].RPC_GiveReward(rank);
            Debug.Log($"🎯 Host: Player {sorted[i].Object.InputAuthority} được hạng {rank}");
        }
    }

    // 👉 RPC gửi xuống client, client tự nhận thưởng
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_GiveReward(int rank)
    {
        switch (rank)
        {
            case 1:
                GoldManager.AddGold(50);
                lightningManeger.AddLightning(20);
                Debug.Log("🎁 Bạn TOP 1: +50 Gold +20 Lightning");
                break;

            case 2:
                GoldManager.AddGold(40);
                lightningManeger.AddLightning(15);
                Debug.Log("🎁 Bạn TOP 2: +40 Gold +15 Lightning");
                break;

            case 3:
                GoldManager.AddGold(30);
                lightningManeger.AddLightning(10);
                Debug.Log("🎁 Bạn TOP 3: +30 Gold +10 Lightning");
                break;

            case 4:
                GoldManager.AddGold(20);
                lightningManeger.AddLightning(5);
                Debug.Log("🎁 Bạn TOP 4: +20 Gold +5 Lightning");
                break;

            default:
                Debug.Log("🙅 Không có phần thưởng cho hạng này");
                break;
        }
    }
}
