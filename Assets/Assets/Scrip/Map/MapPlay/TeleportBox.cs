using UnityEngine;

public class TeleportBox : MonoBehaviour
{
    private MapManager mapManager;

    private void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mapManager.MoveToNextMap();
        }
    }
}