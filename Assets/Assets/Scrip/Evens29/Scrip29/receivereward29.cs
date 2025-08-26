using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class receivereward29 : MonoBehaviour
{
    [Header("Button nhận thưởng")]
    public Button rewardButton;

    private bool hasClaimed = false; // Đã nhận chưa

    public void Top1()
    {
        if (hasClaimed) return; // Nếu đã nhận rồi thì thoát

        CardsManeger.AddCards(5);
        lightningManeger.AddLightning(20);
        GoldManager.AddGold(500);

        EndClaim();
    }

    public void Top2()
    {
        if (hasClaimed) return;

        CardsManeger.AddCards(5);
        lightningManeger.AddLightning(15);
        GoldManager.AddGold(500);

        EndClaim();
    }

    public void Top3()
    {
        if (hasClaimed) return;

        CardsManeger.AddCards(3);
        lightningManeger.AddLightning(15);
        GoldManager.AddGold(300);

        EndClaim();
    }

    public void Top4()
    {
        if (hasClaimed) return;

        CardsManeger.AddCards(3);
        lightningManeger.AddLightning(10);
        GoldManager.AddGold(200);

        EndClaim();
    }

    public void Top5()
    {
        if (hasClaimed) return;

        CardsManeger.AddCards(2);
        lightningManeger.AddLightning(5);
        GoldManager.AddGold(100);

        EndClaim();
    }

    private void EndClaim()
    {
        hasClaimed = true;
        if (rewardButton != null)
            rewardButton.interactable = false; // Tắt nút
    }

    public void OfPopup()
    {
        gameObject.SetActive(false);
    }
}