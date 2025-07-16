using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public TextMeshProUGUI chatContent;

    private void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
    }

    private void SendMessage()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            ChatManager.Instance.SendChatMessage(message);
            inputField.text = ""; // Xóa nội dung sau khi gửi
        }
    }
}
