using UnityEngine;
using Fusion;
public class PlayerCameraBinder2 : NetworkBehaviour
{
    [Header("Child target cho camera (nếu trống sẽ dùng transform của player)")]
    public Transform followTarget;

    [Header("Tuỳ chọn: bật AudioListener chỉ cho local")]
    public AudioListener localAudio;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Bind();
            if (localAudio) localAudio.enabled = true;
        }
        else
        {
            if (localAudio) localAudio.enabled = false;
        }
    }

    void Bind()
    {
        var t = followTarget ? followTarget : transform;
        if (CameraController.Instance)
        {
            CameraController.Instance.SetTarget(t);
        }
        else
        {
            Debug.LogWarning("CameraController chưa sẵn, camera sẽ bind khi scene có controller.");
        }
    }
}
