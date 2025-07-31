using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (pausePanel.activeSelf)
            ResumeGame();
        else
            OpenPause();
    }

    public void OpenPause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GiveUpToMainMenu()
    {
        Time.timeScale = 1f;

        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            // Stop the network runner cleanly
            runner.Shutdown(true, ShutdownReason.Ok);
        }

        // Delay one frame to make sure shutdown is done
        StartCoroutine(LoadMainMenuNextFrame());
    }

    private System.Collections.IEnumerator LoadMainMenuNextFrame()
    {
        yield return null; // đợi 1 frame
        SceneManager.LoadScene("SampleScene");
    }

    public void OnSettingButtonClick()
    {
        OpenPause();
    }
}
