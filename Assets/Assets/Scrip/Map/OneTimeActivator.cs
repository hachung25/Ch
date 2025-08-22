using UnityEngine;

public class OneTimeActivator : MonoBehaviour
{
    [Header("GameObject chỉ định (kéo vào đây)")]
    public GameObject targetObject;
    public GameObject Playerrr;

    private string saveKey = "MyObjectActivated";

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("⚠ Chưa gán GameObject nào vào OneTimeActivator!");
            return;
        }

        // Kiểm tra trạng thái đã lưu
        if (PlayerPrefs.GetInt(saveKey, 0) == 0)
        {
            // Lần đầu tiên -> bật
            targetObject.SetActive(true);
            Debug.Log("🟢 Trường hợp 1: Bật lần đầu tiên!");

            // Lưu trạng thái
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            
            targetObject.SetActive(false);
            Playerrr.SetActive(true);
        }
    }
}
