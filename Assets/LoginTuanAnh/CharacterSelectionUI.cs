using UnityEngine;
using TMPro;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Text UI")]
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI factionText;

    [Header("Panels")]
    public GameObject createPlayerPanel;
    public GameObject selectPlayerPanel;

    private string[] factions = { "Chiến binh", "Thuật sư", "Sát thủ" };
    private int currentFactionIndex = 0;

    public void ToggleGender()
    {
        genderText.text = (genderText.text == "Nam") ? "Nữ" : "Nam";
    }

    public void ToggleFaction()
    {
        currentFactionIndex = (currentFactionIndex + 1) % factions.Length;
        factionText.text = factions[currentFactionIndex];
    }

    /// <summary>
    /// Khi nhấn "Tạo mới": ẩn panel tạo nhân vật, hiện panel chọn nhân vật
    /// </summary>
    public void OnCreateNewButtonClicked()
    {
        if (createPlayerPanel != null) createPlayerPanel.SetActive(false);
        if (selectPlayerPanel != null) selectPlayerPanel.SetActive(true);
    }

    /// <summary>
    /// Khi nhấn "Thoát": ẩn panel chọn nhân vật, hiện lại panel tạo nhân vật
    /// </summary>
    public void OnBackButtonClicked()
    {
        if (selectPlayerPanel != null) selectPlayerPanel.SetActive(false);
        if (createPlayerPanel != null) createPlayerPanel.SetActive(true);
    }
}
