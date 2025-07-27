using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardsDisplay : MonoBehaviour
{
    public TextMeshProUGUI CardText;
    public Slider slider;
    public int maxCards = 100; // bạn có thể điều chỉnh hoặc set bằng code nếu cần

    void OnEnable()
    {
        CardsManeger.OnCardsChanged += UpdateCardsUI;
        UpdateCardsUI(CardsManeger.GetCards());
    }

    void OnDisable()
    {
        CardsManeger.OnCardsChanged -= UpdateCardsUI;
    }

    public void UpdateCardsUI(int cards)
    {
        if (CardText != null)
            CardText.text = $"{cards}";

        if (slider != null)
        {
            slider.maxValue = maxCards;
            slider.value = cards;
        }
    }
}