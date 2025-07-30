using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class MyFusionLauncher : MonoBehaviour
{
    public NetworkRunner runnerPrefab;
    public Button playButton;

    void Start()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(true); // Bật nút khi game bắt đầu
    }

    public void StartSharedClient()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(false); // Ẩn nút sau khi bấm

        var runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;

        runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "DefaultRoom",
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });
    }
}
