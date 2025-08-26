using Fusion;
using UnityEngine;

[RequireComponent(typeof(NicknameSync))]
public class PlayerNameBootstrap : NetworkBehaviour
{
    private NicknameSync _nick;

    void Awake() => _nick = GetComponent<NicknameSync>();

    public override void Spawned()
    {
        // Host (StateAuthority) gán trực tiếp để thấy tên ngay
        if (HasStateAuthority && _nick != null)
            _nick.Nickname = PlayerName.Current;

        // Client (chỉ InputAuthority) yêu cầu server set
        if (HasInputAuthority && !HasStateAuthority && _nick != null)
            _nick.RPC_SetNickname(PlayerName.Current);

        // Nếu đổi tên sau spawn -> cập nhật lại
        if (HasInputAuthority)
            PlayerName.OnChanged += HandleLocalNameChanged;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
            PlayerName.OnChanged -= HandleLocalNameChanged;
    }

    void HandleLocalNameChanged(string newName)
    {
        if (_nick == null) return;

        if (HasStateAuthority) _nick.Nickname = newName;         // host
        else if (HasInputAuthority) _nick.RPC_SetNickname(newName); // client
    }
}
