
using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetSetter : MonoBehaviour
{
    public CinemachineCamera virtualCamera;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (virtualCamera != null && player != null)
        {
            virtualCamera.Follow = player.transform;
            virtualCamera.LookAt = player.transform;
        }
    }
}
