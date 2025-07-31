using UnityEngine;

public class TeleportBox : MonoBehaviour
{
    private MapManager mapManager;

    private void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Player collided with teleport box");
            mapManager.MoveToNextMap();
        }
    }
}