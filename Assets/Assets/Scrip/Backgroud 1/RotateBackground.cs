using UnityEngine;

public class ScrollBackgroundLoop : MonoBehaviour
{
    public RectTransform bg1;
    public RectTransform bg2;
    public float scrollSpeed = 100f;

    private float width;

    void Start()
    {
        width = bg1.rect.width;
    }

    void Update()
    {
        Scroll(bg1);
        Scroll(bg2);

        CheckReset(bg1, bg2);
        CheckReset(bg2, bg1);
    }

    void Scroll(RectTransform bg)
    {
        bg.anchoredPosition += new Vector2(scrollSpeed * Time.deltaTime, 0);
    }

    void CheckReset(RectTransform current, RectTransform other)
    {
        if (current.anchoredPosition.x >= width)
        {
            // đặt lại vị trí hiện tại về phía sau ảnh kia
            current.anchoredPosition = new Vector2(other.anchoredPosition.x - width, 0);
        }
    }
}