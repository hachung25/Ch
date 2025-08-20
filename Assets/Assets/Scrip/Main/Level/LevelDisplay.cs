using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelDisplay : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Slider slider;
    public int maxlevel = 30; 

    void OnEnable()
    {
        LevelManager.OnLevelChanged += UpdateLevelUI;
        UpdateLevelUI(LevelManager.GetLevel()); // hiển thị ngay level khi mở game
    }

    void OnDisable()
    {
        LevelManager.OnLevelChanged -= UpdateLevelUI;
    }

    void UpdateLevelUI(int level)
    {
        if (levelText != null)
            levelText.text = $"{level} /30";
        
        if (slider != null)
        {
            slider.maxValue = maxlevel;
            slider.value = level;
        }
    }
}