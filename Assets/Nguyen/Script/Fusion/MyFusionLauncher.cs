using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class MyFusionLauncher : MonoBehaviour
{
    public NetworkRunner runnerPrefab;
    public Button playButton;

    async void Start()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(true);

        // 👉 Auto start nếu muốn
        StartSharedClient();
    }

    public async void StartSharedClient()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(false);

        var runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;

        // ✅ Gán SceneManager đúng cách để Fusion quản lý scene
        var sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "DefaultRoom",
            SceneManager = sceneManager // 🔥 Phần quan trọng nhất!
        });

        if (result.Ok)
        {
            Debug.Log("✅ Fusion khởi động thành công bằng MyFusionLauncher.");
        }
        else
        {
            Debug.LogError("❌ Fusion khởi động thất bại: " + result.ShutdownReason);
        }
    }
}
