using UnityEngine;

public class OpenChat : MonoBehaviour
{
    public GameObject Chat;

    public void openChat()
    {
        if (Chat != null)
        {
            Chat.SetActive(true); // ✅ Chỉ mở nếu còn tồn tại
        }
        else
        {
            Debug.LogWarning("Chat object is null. Possibly destroyed?");
        }
    }

    public void close()
    {
        if (Chat != null)
        {
            Chat.SetActive(false); // ✅ Chỉ đóng nếu còn tồn tại
        }
    }
}
