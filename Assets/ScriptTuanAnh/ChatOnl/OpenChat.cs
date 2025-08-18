using Photon.Chat;
using UnityEngine;

public class OpenChat : MonoBehaviour
{
    public GameObject Chat;

    public void openChat()
    {
        if (Chat != null)
        {
            Chat.SetActive(true);
            ChatState.IsChatting = true; // 🛑 Bắt đầu chat → chặn input
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
            Chat.SetActive(false);
            ChatState.IsChatting = false; // ✅ Đóng chat → cho phép input
        }
    }

}
