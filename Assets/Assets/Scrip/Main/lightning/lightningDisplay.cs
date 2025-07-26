using UnityEngine;
using TMPro;

public class lightningDisplay : MonoBehaviour
{
    public TextMeshProUGUI lightningText;

    void OnEnable()
    {
        lightningManeger.OnLightningChanged += UpdateLightningUI;
        UpdateLightningUI(lightningManeger.GetLightning());
    }

    void OnDisable()
    {
        lightningManeger.OnLightningChanged -= UpdateLightningUI;
    }

    void UpdateLightningUI(int lightning)
    {
        if (lightningText != null)
            lightningText.text = $"{lightning}";
    }
}