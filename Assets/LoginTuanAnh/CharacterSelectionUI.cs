using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Text UI")]
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI factionText;

    [Header("Panels")]
    public GameObject createPlayerPanel;
    public GameObject selectPlayerPanel;

    [Header("Faction Images")]
    public Image[] factionImages;
    public Color selectedColor = Color.green;
    public Color normalColor = Color.white;

    [Header("Image Size Settings")]
    public Vector2 selectedSize = new Vector2(160, 160); // Kích thước ảnh được chọn
    public Vector2 normalSize = new Vector2(100, 100);   // Kích thước mặc định

    [Header("Chọn Button")]
    public Button chooseButton;

    [Header("Scene")]
    public string nextSceneName = "gamePlay";

    private int selectedImageIndex = -1; // Ban đầu chưa chọn gì
    private string[] factions = { "Chiến binh", "Thuật sư", "Sát thủ" };
    private int currentFactionIndex = 0;

    void Start()
    {
        chooseButton.interactable = false;
        factionText.text = factions[currentFactionIndex]; // Hiển thị mặc định
    }

    public void ToggleGender()
    {
        genderText.text = (genderText.text == "Nam") ? "Nữ" : "Nam";
    }

    public void ToggleFaction()
    {
        currentFactionIndex = (currentFactionIndex + 1) % factions.Length;
        factionText.text = factions[currentFactionIndex];
    }

    public void OnCreateNewButtonClicked()
    {
        if (createPlayerPanel != null) createPlayerPanel.SetActive(false);
        if (selectPlayerPanel != null) selectPlayerPanel.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        if (selectPlayerPanel != null) selectPlayerPanel.SetActive(false);
        if (createPlayerPanel != null) createPlayerPanel.SetActive(true);
    }

    public void OnFactionImageClicked(int index)
    {
        if (index < 0 || index >= factionImages.Length) return;

        for (int i = 0; i < factionImages.Length; i++)
        {
            bool isSelected = (i == index);

            // Tween màu
            factionImages[i].DOColor(isSelected ? selectedColor : normalColor, 0.25f);

            // Tween kích thước
            RectTransform rt = factionImages[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.DOSizeDelta(isSelected ? selectedSize : normalSize, 0.25f).SetEase(Ease.OutBack);
            }
        }

        selectedImageIndex = index;
        currentFactionIndex = index;
        factionText.text = factions[currentFactionIndex];
        chooseButton.interactable = true;
    }

    public void OnChooseButtonClicked()
    {
        if (selectedImageIndex < 0) return;

        Debug.Log("Selected faction: " + factions[selectedImageIndex]);
        SceneManager.LoadScene(nextSceneName);
    }
}
