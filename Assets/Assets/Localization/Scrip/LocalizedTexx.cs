using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTexx : MonoBehaviour
{
    public string key; // Khóa tra trong bảng

    private LocalizeStringEvent localizeEvent;
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        // Thêm LocalizeStringEvent nếu chưa có
        localizeEvent = gameObject.GetComponent<LocalizeStringEvent>();
        if (localizeEvent == null)
        {
            localizeEvent = gameObject.AddComponent<LocalizeStringEvent>();
        }

        // Gán bảng và key
        localizeEvent.StringReference.TableReference = "LocalizatioN";
        localizeEvent.StringReference.TableEntryReference = key;

        // Gắn hàm xử lý sự kiện khi giá trị dịch thay đổi
        localizeEvent.OnUpdateString.AddListener(UpdateText);
    }

    private void UpdateText(string localizedValue)
    {
        if (textComponent != null)
        {
            textComponent.text = localizedValue;
        }
    }
}