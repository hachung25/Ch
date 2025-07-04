using UnityEngine;

public class PanelToggle : MonoBehaviour
{
    public GameObject panel; // Kéo Panel muốn bật/tắt vào đây

    // Gọi hàm này từ nút bấm
    public void TogglePanel()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf); // Đảo trạng thái
        }
    }
}