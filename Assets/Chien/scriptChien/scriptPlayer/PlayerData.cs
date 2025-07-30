using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    // Dữ liệu cần giữ lại
    //public int playerHealth = 100;
    public Vector2 savedPosition;

    private void Awake()
    {
        // Đảm bảo chỉ có 1 Instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Không bị xóa khi load scene mới
    }
}

