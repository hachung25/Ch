using UnityEngine;
using TMPro;

public class CardsDisplay : MonoBehaviour
{
    public TextMeshProUGUI CardText;

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
    }
}
