using Fusion;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [Networked] public int CharacterIndex { get; set; }

    [SerializeField] private GameObject[] visuals;

    private int _lastApplied = -1;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            int idx = 0;
            if (CharacterSelectionManager.Instance != null)
                idx = Mathf.Max(0, CharacterSelectionManager.Instance.selectedCharacterIndex);

            RPC_SetCharacter(idx);
        }

        SafeApply();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCharacter(int idx)
    {
        if (visuals == null || visuals.Length == 0) { CharacterIndex = 0; return; }
        CharacterIndex = Mathf.Clamp(idx, 0, visuals.Length - 1);
    }

    public override void Render()
    {
        SafeApply();
    }

    void SafeApply()
    {
        if (visuals == null || visuals.Length == 0) return;
        int idx = Mathf.Clamp(CharacterIndex, 0, visuals.Length - 1);
        if (idx != CharacterIndex) CharacterIndex = idx;

        if (_lastApplied == idx) return;
        _lastApplied = idx;

        for (int i = 0; i < visuals.Length; i++)
            if (visuals[i]) visuals[i].SetActive(i == idx);
    }
}
