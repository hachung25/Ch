using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using System.Collections;

public class LanguageController : MonoBehaviour
{
    private void Start()
    {
        // Khi game chạy, lấy ngôn ngữ đã lưu. Nếu chưa có thì dùng "en"
        string savedLang = PlayerPrefs.GetString("language", "en");
        StartCoroutine(SetLanguage(savedLang));
    }

    public void ChangeLanguage(string code)
    {
        // Khi người chơi chọn ngôn ngữ, lưu vào PlayerPrefs
        PlayerPrefs.SetString("language", code);
        PlayerPrefs.Save();

        // Áp dụng ngôn ngữ mới
        StartCoroutine(SetLanguage(code));
        Debug.Log("Đã chuyển ngôn ngữ sang: " + code);
    }

    private IEnumerator SetLanguage(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var selected = locales.Find(l => l.Identifier.Code == code);

        if (selected != null)
        {
            LocalizationSettings.SelectedLocale = selected;

            // Sau khi đổi locale, cập nhật lại toàn bộ LocalizedTexx đang có
            RefreshAllLocalizedTexts();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy ngôn ngữ: " + code);
        }
    }

    private void RefreshAllLocalizedTexts()
    {
        // Tìm tất cả đối tượng đang dùng LocalizedTexx (kể cả đang ẩn)
        var localizedTexts = FindObjectsOfType<LocalizedTexx>(true);
        foreach (var text in localizedTexts)
        {
            text.RefreshText(); // Gọi hàm cập nhật lại nội dung dịch
        }

        Debug.Log("Đã cập nhật lại toàn bộ text sau khi đổi ngôn ngữ.");
    }
}