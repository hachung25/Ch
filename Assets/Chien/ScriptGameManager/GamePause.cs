using UnityEngine;
using UnityEngine.Timeline;
using System.Collections;
using UnityEngine.SceneManagement;
public class GamePause : MonoBehaviour
{
    public GameObject PauseMain;
    public string GameHome;

    public void Start()
    {
        Time.timeScale = 1.0f;
    }
    public void loadPause()
    {
        PauseMain.SetActive(true);
        Time.timeScale = 0f;
        
    }
    public void continueGame()
    {
        PauseMain.SetActive(false);
        Time.timeScale = 1f;
    }

    public void BackHome()
    {
        SceneManager.LoadScene(GameHome);
    }
    public void OnRetryButton()
    {
        // Reset vị trí player
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.savedPosition = PlayerData.Instance.defaultPosition;
            PlayerData.Instance.ResetPlayer();
        }

        // Reset enemy như hướng dẫn trước
        foreach (var enemy in FindObjectsOfType<EnemyResetPoint>())
        {
            enemy.ResetEnemy();
        }
        PauseMain.SetActive(false);
        Time.timeScale = 1f;
    }
    public void ReloadScene1()
    {
        SceneManager.LoadScene("Mode1");
    } public void ReloadScene2()
    {
        SceneManager.LoadScene("Mode2");
    }

}
