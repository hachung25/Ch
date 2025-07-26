using Fusion;
using TMPro;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public string PlayerName { get; set; }

    [SerializeField] private TextMeshProUGUI nameTag; // <-- Drag Text từ Canvas (Text "Name") vào đây

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player" + Random.Range(1000, 9999));
            RPC_SetPlayerName(savedName);
        }

        // Gán tên vào UI (nếu đã có)
        if (nameTag != null)
            nameTag.text = PlayerName;
    }

    public override void FixedUpdateNetwork()
    {
        // Cập nhật tên liên tục nếu cần (hoặc bỏ nếu không cần)
        if (nameTag != null)
            nameTag.text = PlayerName;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }
}
