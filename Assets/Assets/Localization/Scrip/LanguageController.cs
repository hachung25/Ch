using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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
        // Khi người chơi chọn ngôn ngữ lưu vào PlayerPrefs
        PlayerPrefs.SetString("language", code);
        PlayerPrefs.Save();

        // Áp dụng ngôn ngữ mới
        StartCoroutine(SetLanguage(code));
        Debug.Log("đã chuyển ngôn ngữ");
    }

    private IEnumerator SetLanguage(string code)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var selected = locales.Find(l => l.Identifier.Code == code);

        if (selected != null)
        {
            LocalizationSettings.SelectedLocale = selected;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy ngôn ngữ: " + code);
        }
    }
}