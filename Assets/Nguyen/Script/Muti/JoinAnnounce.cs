using Fusion;
using UnityEngine;

public class JoinAnnounce : NetworkBehaviour
{
    // Cho phép mọi proxy (client) gọi vào StateAuthority (server)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_Announce(string firebaseUid, int characterIndex, RpcInfo info = default)
    {
        FusionIdentityBridge.PlayerToFirebaseUid[info.Source] = firebaseUid;
        FusionIdentityBridge.PlayerToCharIndex[info.Source] = characterIndex;
        Debug.Log($"[JoinAnnounce] Map {info.Source} -> {firebaseUid}, char={characterIndex}");
    }
}
