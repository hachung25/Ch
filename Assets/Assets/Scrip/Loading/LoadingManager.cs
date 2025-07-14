using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public GameObject login;
    public GameObject loading;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    public float loadSpeed = 1f;
    private float currentProgress = 0f;
    private bool isLoadingDone = false;

    void Start()
    {
        login.SetActive(false);
        loading.SetActive(true);
        loadingSlider.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        StartCoroutine(LoadingRoutine());
    }
    void Update()
    {
        AnimateText();
    }

    IEnumerator LoadingRoutine()
    {
        // Chờ 10 giây
        yield return new WaitForSeconds(5f);

        // Bật slider và text lên
        loadingSlider.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);

        bool hasPausedAt80 = false;

        while (currentProgress < 100f)
        {
            if (currentProgress < 80f)
            {
                currentProgress += Time.deltaTime * loadSpeed * 20;
            }
            else if (!hasPausedAt80)
            {
                currentProgress = 80f;
                loadingSlider.value = currentProgress / 100f;
                hasPausedAt80 = true;
                yield return new WaitForSeconds(3f); // CHỜ 3 GIÂY
            }
            else
            {
                currentProgress += Time.deltaTime * loadSpeed * 20;
            }

            currentProgress = Mathf.Clamp(currentProgress, 0, 100);
            loadingSlider.value = currentProgress / 100f;

            yield return null;
        }

        isLoadingDone = true;
        login.SetActive(true);
        loading.SetActive(false);
    }

    void AnimateText()
    {
        if (loadingText == null) return;

        // Scale phóng to thu nhỏ liên tục
        float scale = 1f + 0.1f * Mathf.Sin(Time.time * 4f);
        loadingText.transform.localScale = new Vector3(scale, scale, 1f);
    }
}