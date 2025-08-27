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

    public void GiveUpToMainMenuOffline()
    {
        // Đảm bảo TimeScale về mặc định
        Time.timeScale = 1f;

        // Gọi Dataload để load dữ liệu trước khi vào scene
        var dataload = FindObjectOfType<Dataload>();
        if (dataload != null)
        {
            dataload.LoadAllDataFromFirebase();
        }
        else
        {
            // Nếu không có Dataload, fallback load scene trực tiếp
            SceneManager.LoadScene("SampleScene");
        }
    }

    public async void GiveUpToMainMenuOnline()
    {
        Time.timeScale = 1f;

        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            // Dừng runner sạch sẽ
            await runner.Shutdown(true, ShutdownReason.Ok);
        }

        // Gọi Dataload để load dữ liệu trước khi vào scene
        var dataload = FindObjectOfType<Dataload>();
        if (dataload != null)
        {
            dataload.LoadAllDataFromFirebase();
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }


    public void OnSettingButtonClick()
    {
        OpenPause();
    }
}
