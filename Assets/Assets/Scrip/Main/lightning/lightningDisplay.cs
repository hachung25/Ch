using UnityEngine;
using TMPro;

public class lightningDisplay : MonoBehaviour
{
    
    public TextMeshProUGUI lightningText;

    void OnEnable()
    {
        lightningManeger.OnlightningChanged += UpdateGoldUI;
        UpdateGoldUI(lightningManeger.GetLightning());
    }

    void OnDisable()
    {
        lightningManeger.OnlightningChanged -= UpdateGoldUI;
    }

    void UpdateGoldUI(int lightning)
    {
        if (lightningText != null)
            lightningText.text = $"{lightning}";
    }
}
