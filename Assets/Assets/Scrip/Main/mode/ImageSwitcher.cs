using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase.Auth;

public class ImageSwitcher : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] sprites;
    public float fadeDuration = 0.5f;
    private int currentIndex = 0;
    private bool isTransitioning = false;

    public GameObject bt1, bt2, bt3;
    public GameObject BtNextleft, BtNextRight;

    public bool Modee;
    public GameObject Unlock;

     void Start()
     {
        UpData();
        BtNextleft.SetActive(false);
     }

    public void UpData()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            FireBaseDataBaseManager db = FindObjectOfType<FireBaseDataBaseManager>();
            if (db != null)
            {
                db.LoadMode(userId, ApplyMode); // callback khi có mode
            }
            else
            {
                Debug.LogWarning("Không tìm thấy FireBaseDataBaseManager.");
            }
        }
        else
        {
            Debug.LogWarning("User chưa đăng nhập.");
        }
    }

 
    private void ApplyMode(bool mode)
    {
       Modee = mode;
       Debug.Log(Modee);
    }

    public void ShowPrevious()
    {
        if (!isTransitioning)
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = sprites.Length - 1;

            StartCoroutine(FadeToSprite(sprites[currentIndex]));
        }
        Unlock.SetActive(false);
        BtNextleft.SetActive(false);
        BtNextRight.SetActive(true);
        
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
        if (Modee == true)
        {
            Unlock.SetActive(false);
        }
        else
        {
            Unlock.SetActive(true);
        }
        BtNextRight.SetActive(false);
       BtNextleft.SetActive(true);
        
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

        // Thay sprite
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

        targetImage.color = originalColor;
        isTransitioning = false;
        
        if (newSprite.name == "Arena2_0")
        {
            bt1.SetActive(false);
            if (Modee == true)
            {
                bt3.SetActive(true);
                Debug.Log("mở mode 2");
            }
            else
            {
                bt2.SetActive(true);
            }
            
        }
        else
        {
            bt1.SetActive(true);
            bt2.SetActive(false);
            bt3.SetActive(false);
        }
    }
}
