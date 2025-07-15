using UnityEngine;
using TMPro;
public class GoldDisplay : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    void OnEnable()
    {
        GoldManager.OnGoldChanged += UpdateGoldUI;
        UpdateGoldUI(GoldManager.GetGold());
    }

    void OnDisable()
    {
        GoldManager.OnGoldChanged -= UpdateGoldUI;
    }

    void UpdateGoldUI(int gold)
    {
        if (goldText != null)
            goldText.text = $"{gold}";
    }
}