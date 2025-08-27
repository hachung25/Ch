using UnityEngine;
using UnityEngine.Timeline;
using System.Collections;
using UnityEngine.SceneManagement;
public class GamePause : MonoBehaviour
{
    public GameObject PauseMain;
    private RoomService _roomService;

    public void Start()
    {
        Time.timeScale = 1.0f;
        _roomService = FindObjectOfType<RoomService>();  // tìm RoomService trong scene
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
      
        SceneManager.LoadScene("SampleScene");

    }

    public void Dele()
    {
        if (_roomService != null)
        {
            _roomService.DeleteRoom();
        }

        var dataload = FindObjectOfType<Dataload>();
        if (dataload != null)
        {
            dataload.LoadAllDataFromFirebase();
        }
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
