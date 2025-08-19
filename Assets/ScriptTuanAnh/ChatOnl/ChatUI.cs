using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public TextMeshProUGUI chatContent;
    public ScrollRect scrollRect;

    private void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(delegate { HandleSubmit(); });
    }

    private void HandleSubmit()
    {
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            SendMessage();
        }
        // Nếu Shift đang được nhấn → cho phép xuống dòng
    }

    public void SendMessage()
    {
        string message = inputField.text;
        if (!string.IsNullOrWhiteSpace(message))
        {
            ChatManager.Instance.SendChatMessage(message.Trim());
            inputField.text = "";
            inputField.ActivateInputField(); // focus lại
        }
    }


}