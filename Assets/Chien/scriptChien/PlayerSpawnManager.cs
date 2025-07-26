using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PlayerData.Instance != null)
        {
            player.transform.position = PlayerData.Instance.savedPosition;
        }
    }
}
