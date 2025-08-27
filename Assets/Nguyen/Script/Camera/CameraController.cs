using UnityEngine;
using Unity.Cinemachine; // nhớ có using này

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    [SerializeField] private CinemachineCamera cmCamera;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (!cmCamera) cmCamera = GetComponentInChildren<CinemachineCamera>();
    }

    public void SetTarget(Transform target)
    {
        if (!cmCamera || !target) return;

        // CM v3: dùng Follow/LookAt
        cmCamera.Follow = target;
        cmCamera.LookAt = target;
    }
}
