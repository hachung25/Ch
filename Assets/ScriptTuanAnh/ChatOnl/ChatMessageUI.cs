using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUI : MonoBehaviour
{
    public TextMeshProUGUI userIDText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI timeText;

    private System.DateTime timeSent;
    private float timeSinceLastUpdate = 0f;

    public void Setup(string userName, string message)
    {
        timeSent = System.DateTime.Now;

        if (userIDText) userIDText.text = userName;
        if (messageText) messageText.text = message;

        UpdateTime();

        // Ép layout tính lại ngay sau khi set text
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= 60f) // Cập nhật mỗi phút
        {
            UpdateTime();
            timeSinceLastUpdate = 0f;
        }
    }

    private void UpdateTime()
    {
        if (timeText == null) return;

        var span = System.DateTime.Now - timeSent;

        if (span.TotalMinutes < 60)
            timeText.text = $"{(int)span.TotalMinutes} min ago";
        else
            timeText.text = $"{(int)span.TotalHours} h ago";
    }

    public void SetAlignment(bool isMine)
    {
        // Nếu bạn muốn căn phải/trái theo người gửi
        // Có thể để trống nếu chưa cần layout thay đổi

        // Ví dụ (nếu dùng HorizontalLayoutGroup):
        // layoutGroup.childAlignment = isMine ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
    }

}
