using System;
using Fusion;
using UnityEngine;

[DisallowMultipleComponent]
public class NicknameSync : NetworkBehaviour
{
    // Server (StateAuthority) ghi, tự replicate tới mọi client
    [Networked]
    public NetworkString<_32> Nickname { get; set; }

    public event Action<string> NicknameChanged;

    private NetworkString<_32> _lastNickname;

    // Gọi mỗi khung hình render ở cả Host/Client → phát hiện thay đổi & bắn event
    public override void Render()
    {
        if (Nickname != _lastNickname)
        {
            _lastNickname = Nickname;
            NicknameChanged?.Invoke(Nickname.ToString());
        }
    }

    // Client (InputAuthority) yêu cầu server set tên
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickname(NetworkString<_32> value)
    {
        Nickname = value;
    }
}
