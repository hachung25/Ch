using System;
using UnityEngine;

public class showpause : MonoBehaviour
{
    public GameObject pauseMenu;
    
    public void showPause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void ofshowpause()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
}
