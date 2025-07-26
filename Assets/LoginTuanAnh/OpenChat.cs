using UnityEngine;

public class OpenChat : MonoBehaviour
{
    public GameObject Chat;

    public void openChat()
    {
        Chat.SetActive(!Chat.activeSelf);
    }
}
