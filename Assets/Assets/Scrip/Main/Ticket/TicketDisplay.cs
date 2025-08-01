using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TicketDisplay : MonoBehaviour
{
    public TextMeshProUGUI ticketText;
    void OnEnable()
    {
        TicketManeger.OnTicketChanged += UpdateTicketUI;
        UpdateTicketUI(CardsManeger.GetCards());
    }

    void OnDisable()
    {
        TicketManeger.OnTicketChanged -= UpdateTicketUI;
    }

    public void UpdateTicketUI(int Tickets)
    {
        if (ticketText != null)
            ticketText.text = $"{Tickets}";
        
    }
}
