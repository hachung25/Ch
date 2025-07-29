using UnityEngine;
using System.Collections;

public class TB : MonoBehaviour
{
    public GameObject tb;
    public GameObject tbs;
    public float scaleDuration = 0.5f;     // Thời gian phóng to và thu nhỏ
    public float waitTime = 0.5f;          // Thời gian chờ của tb
    public float waitTimeTbs = 2.5f;       // Thời gian chờ riêng cho tbs

    private Vector3 originalScaleTb;
    private Vector3 originalScaleTbs;
    private bool isRunningTb = false;
    private bool isRunningTbs = false;

    void Start()
    {
        originalScaleTb = tb.transform.localScale;
        originalScaleTbs = tbs.transform.localScale;
        tb.SetActive(false);
        tbs.SetActive(false);
    }

    public void showTb()
    {
        if (!isRunningTb)
        {
            StartCoroutine(ShowWithScaleEffect(tb, originalScaleTb, waitTime, () => isRunningTb = false));
            isRunningTb = true;
        }
    }

    public void ShowTbs()
    {
        if (!isRunningTbs)
        {
            StartCoroutine(ShowWithScaleEffect(tbs, originalScaleTbs, waitTimeTbs, () => isRunningTbs = false));
            isRunningTbs = true;
        }
    }

    private IEnumerator ShowWithScaleEffect(GameObject obj, Vector3 targetScale, float wait, System.Action onComplete)
    {
        obj.SetActive(true);
        obj.transform.localScale = Vector3.zero;

        // Phóng to
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsed / scaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = targetScale;

        yield return new WaitForSeconds(wait);

        // Thu nhỏ
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            obj.transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, elapsed / scaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = Vector3.zero;

        obj.SetActive(false);
        onComplete?.Invoke();
    }
}
