using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImageSwitcher : MonoBehaviour
{
    public Image targetImage;              // Ảnh chính để hiển thị
    public Sprite[] sprites;               // Mảng ảnh
    public float fadeDuration = 0.5f;      // Thời gian chuyển ảnh
    private int currentIndex = 0;
    private bool isTransitioning = false;

    public GameObject bt1, bt2;

    public void ShowPrevious()
    {
        if (!isTransitioning)
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = sprites.Length - 1;

            StartCoroutine(FadeToSprite(sprites[currentIndex]));
        }
    }

    public void ShowNext()
    {
        if (!isTransitioning)
        {
            currentIndex++;
            if (currentIndex >= sprites.Length)
                currentIndex = 0;
            StartCoroutine(FadeToSprite(sprites[currentIndex]));
        }
    }

    private IEnumerator FadeToSprite(Sprite newSprite)
    {
        isTransitioning = true;
        
        // Fade out
        float elapsed = 0f;
        Color originalColor = targetImage.color;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Thay sprite mới
        targetImage.sprite = newSprite;

        // Fade in
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo alpha = 1 sau cùng
        targetImage.color = originalColor;
        isTransitioning = false;
        
        if (newSprite.name == "Arena2_0")
        {
            bt2.SetActive(true);
            bt1.SetActive(false);
        }
        else
        {
            bt1.SetActive(true);
            bt2.SetActive(false);
        }
    }

}