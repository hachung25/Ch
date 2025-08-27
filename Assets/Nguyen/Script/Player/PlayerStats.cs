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
}
