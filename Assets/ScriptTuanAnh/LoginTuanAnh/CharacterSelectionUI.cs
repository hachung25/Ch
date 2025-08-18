using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject PlayMode;
    public GameObject CanvasCreatePlayer;

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
    
    private int currentFactionIndex = 0;

    void Start() 
    {
        chooseButton.interactable = false;
        
    }
    public void OnBackButtonClicked()
    {
        if (CanvasCreatePlayer != null) CanvasCreatePlayer.SetActive(false);
        if (PlayMode != null) PlayMode.SetActive(true);
    }

    public void OnFactionImageClicked(int index)
    {
        if (index < 0 || index >= factionImages.Length) return;

        // Nếu đang chọn lại chính ảnh đó → bỏ chọn
        if (selectedImageIndex == index)
        {
            // Về lại trạng thái ban đầu
            factionImages[index].DOColor(normalColor, 0.25f);
            RectTransform rt = factionImages[index].GetComponent<RectTransform>();
            if (rt != null)
                rt.DOSizeDelta(normalSize, 0.25f).SetEase(Ease.OutBack);

            selectedImageIndex = -1;
            
            chooseButton.interactable = false;
            return;
        }

        // Chọn ảnh mới → xử lý toàn bộ
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
        
        chooseButton.interactable = true;
    }


    public void OnConfirmButtonClick()
    {
        // Kiểm tra xem đã chọn nhân vật chưa
        if (CharacterSelectionManager.Instance != null && CharacterSelectionManager.Instance.selectedCharacterIndex != -1)
        {
            // Nếu đã chọn, chuyển sang scene tiếp theo
            SceneManager.LoadScene("TestMuti");
        }
        else
        {
            Debug.Log("Vui lòng chọn một nhân vật trước khi xác nhận!");
            // Tùy chọn: hiển thị một thông báo cho người dùng
        }
    }
}
