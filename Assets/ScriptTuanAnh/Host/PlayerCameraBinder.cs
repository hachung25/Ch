using Fusion;
using UnityEngine;

public class PlayerCameraBinder : NetworkBehaviour
{
    public override void Spawned()
    {
        if (Object.HasInputAuthority && CameraFollow.Instance != null)
        {
            CameraFollow.Instance.target = transform;
        }
    }
}
