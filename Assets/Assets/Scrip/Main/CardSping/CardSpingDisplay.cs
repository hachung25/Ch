using UnityEngine;
using TMPro;


public class CardSpingDisplay : MonoBehaviour
{
    public TextMeshProUGUI ticketText;
    void OnEnable()
    {
        CardSpingManeger.OnCardSpingChanged += UpdateTicketUI;
        UpdateTicketUI(CardSpingManeger.GetCardSping());
    }

    void OnDisable()
    {
        CardSpingManeger.OnCardSpingChanged -= UpdateTicketUI;
    }

    public void UpdateTicketUI(int Cardsping)
    {
        if (ticketText != null)
            ticketText.text = $"{Cardsping}";
        
    }
}
