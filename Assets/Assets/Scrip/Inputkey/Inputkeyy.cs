using UnityEngine;
using UnityEngine.UI;
public class Inputkeyy : MonoBehaviour
{
    [System.Serializable]
    public class KeyImage
    {
        public KeyCode key;   // Phím cần check
        public Image image;   // Ảnh tương ứng
    }

    public Color pressedColor = Color.yellow;  // Màu khi nhấn
    public Color normalColor = Color.white;    // Màu bình thường
    public KeyImage[] keyImages;               // Kéo 5 cái image vào đây + gán phím

    void Update()
    {
        foreach (var ki in keyImages)
        {
            if (ki.image == null) continue;

            if (Input.GetKey(ki.key))
            {
                // Đang nhấn phím → đổi màu
                ki.image.color = pressedColor;
            }
            else
            {
                // Không nhấn → về màu bình thường
                ki.image.color = normalColor;
            }
        }
    }
}
