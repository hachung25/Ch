using System;
using UnityEngine;

public class CancelTheLevel : MonoBehaviour
{
    public GameObject pauseMenu;
    private PlayerHealth playerHealth;
    public MapSpawner mapSpawner;
    
    public void CancelLevel()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        pauseMenu.SetActive(false);

        if (playerHealth != null)
        {
            playerHealth.ResetHealth(); // hoặc gọi hàm bất kỳ bạn muốn
        }

        mapSpawner.BackToHome();
    }
}
