using Fusion;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [Networked] public int CharacterIndex { get; set; }

    [SerializeField] private GameObject[] visuals;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            int idx = 0;
            if (CharacterSelectionManager.Instance != null)
                idx = Mathf.Max(0, CharacterSelectionManager.Instance.selectedCharacterIndex);

            RPC_SetCharacter(idx);
        }

        ApplyVisual();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCharacter(int idx)
    {
        CharacterIndex = Mathf.Clamp(idx, 0, visuals.Length - 1);
    }

    public override void Render()
    {
        ApplyVisual();
    }

    void ApplyVisual()
    {
        for (int i = 0; i < visuals.Length; i++)
            if (visuals[i]) visuals[i].SetActive(i == CharacterIndex);
    }
}
