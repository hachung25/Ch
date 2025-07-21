using UnityEngine;
using System.Collections;

public class TB : MonoBehaviour
{
    public GameObject tb;
    public float scaleDuration = 0.5f;     // Thời gian phóng to và thu nhỏ
    public float waitTime = 0.5f;          // Thời gian chờ sau khi phóng to

    private Vector3 originalScale;
    private bool isRunning = false;        // Cờ kiểm tra hiệu ứng đang chạy

    void Start()
    {
        originalScale = tb.transform.localScale;
        tb.SetActive(false); // Ẩn khi bắt đầu
    }

    public void showTb()
    {
        if (!isRunning)
        {
            StartCoroutine(ShowWithScaleEffect());
        }
    }

    private IEnumerator ShowWithScaleEffect()
    {
        isRunning = true;

        tb.SetActive(true);
        tb.transform.localScale = Vector3.zero; // Bắt đầu từ kích thước nhỏ

        // Phóng to
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            tb.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / scaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        tb.transform.localScale = originalScale;

        yield return new WaitForSeconds(waitTime);

        // Thu nhỏ lại
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            tb.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / scaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        tb.transform.localScale = Vector3.zero;

        tb.SetActive(false);
        isRunning = false;
    }
}