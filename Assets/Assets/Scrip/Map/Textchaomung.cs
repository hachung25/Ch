using UnityEngine;
using System.Collections;
using TMPro;

public class Textchaomung : MonoBehaviour
{
    public TMP_Text textUI;          // TextMeshPro để hiển thị
    [TextArea] public string[] texts; // Danh sách các đoạn text (0 = auto start, 1-2 = bấm button)
    public float delay = 0.05f;      // Tốc độ hiển thị chữ

    private Coroutine typingCoroutine;
    private int currentIndex = 0;    // Vị trí đoạn text hiện tại
    private bool isTyping = false;   // Kiểm tra có đang chạy hiệu ứng không
    public GameObject Popupchaomung;
    public GameObject Playerr;

    void Start()
    {
        // Chạy đoạn text đầu tiên khi vào game
        PlayText(currentIndex);
       Playerr.SetActive(false);
    }

    // Gọi hàm này trong OnClick() của Button
    public void NextText()
    {
        if (isTyping)
        {
            // Nếu đang chạy hiệu ứng thì bỏ qua, hiện ngay full text
            StopCoroutine(typingCoroutine);
            textUI.text = texts[currentIndex];
            isTyping = false;
        }
        else
        {
            // Chuyển sang text tiếp theo nếu còn
            if (currentIndex < texts.Length - 1)
            {
                currentIndex++;
                PlayText(currentIndex);
            }
            else
            {
               Popupchaomung.SetActive(false);
               Time.timeScale = 1;
               Playerr.SetActive(true);
              
            }
        }
    }

    private void PlayText(int index)
    {
        if (index < 0 || index >= texts.Length) return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(ShowText(texts[index]));
    }

    IEnumerator ShowText(string fullText)
    {
        isTyping = true;
        textUI.text = "";
        foreach (char c in fullText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(delay);
        }
        isTyping = false;
    }
}