using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    public string nextSceneName; // Gán tên scene kế tiếp trong Inspector
    public Vector2 spawnPositionInNextScene; // Vị trí spawn trong scene mới

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Lưu trạng thái trước khi chuyển scene
            PlayerData.Instance.savedPosition = spawnPositionInNextScene;

            // Load scene mới
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
