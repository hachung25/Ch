using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerNameFetcher : MonoBehaviour
{
    private NameManager nameManager;

    [Header("UI để hiển thị tên người chơi")]
    public TextMeshProUGUI displayText;

    [Header("Thời gian chờ trước khi lấy tên (giây)")]
    public float delayBeforeFetch = 2f;

    private void Start()
    {
        StartCoroutine(FetchPlayerNameAfterDelay());
    }

    private IEnumerator FetchPlayerNameAfterDelay()
    {
        // Chờ 1 thời gian để NameManager kịp load từ Firebase
        yield return new WaitForSeconds(delayBeforeFetch);

        nameManager = FindObjectOfType<NameManager>();

        if (nameManager == null)
        {
            Debug.LogError("❌ Không tìm thấy NameManager trong scene!");
            yield break;
        }

        string fetchedName = "";

        // Ưu tiên lấy name từ nameTextUpdate nếu có
        if (nameManager.nameTextUpdate != null && !string.IsNullOrEmpty(nameManager.nameTextUpdate.text))
        {
            fetchedName = nameManager.nameTextUpdate.text;
        }
        else if (nameManager.nameText != null && !string.IsNullOrEmpty(nameManager.nameText.text))
        {
            fetchedName = nameManager.nameText.text;
        }

        if (string.IsNullOrEmpty(fetchedName))
        {
            Debug.LogWarning("⚠️ Tên người chơi đang trống (có thể Firebase chưa load xong).");
            displayText.text = "Không có tên!";
        }
        else
        {
            Debug.Log("✅ Tên người chơi lấy được: " + fetchedName);
            if (displayText != null)
                displayText.text = fetchedName;
        }
    }
}
