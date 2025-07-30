using UnityEngine;

public class OpenChat : MonoBehaviour
{
    public GameObject Chat;

    public void openChat()
    {
        Chat.SetActive(true); // Luôn mở chat
    }

    public void close()
    {
        Chat.SetActive(false); // Luôn đóng chat
    }
}
