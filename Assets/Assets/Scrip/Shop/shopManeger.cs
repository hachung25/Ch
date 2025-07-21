using UnityEngine;
using UnityEngine.UI;

public class shopManeger : MonoBehaviour
{
    public Button myButton; 
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f); // màu tối

    private void Start()
    {
        if (PlayerPrefs.GetInt("slot1_claimed", 0) == 1)
        {
            DisableButton();
        }
    }

    public void slot()
    {
        if (PlayerPrefs.GetInt("slot1_claimed", 0) == 1)
        {
            Debug.Log("Slot 1 đã được nhận trước đó.");
            return;
        }

        Debug.Log("slot called");
        GoldManager.AddGold(10);
        PlayerPrefs.SetInt("slot1_claimed", 1); // lưu lại trạng thái đã nhận
        PlayerPrefs.Save(); // lưu vào ổ cứng
        DisableButton();
    }

    private void DisableButton()
    {
        if (myButton != null)
        {
            myButton.interactable = false;
            ColorBlock cb = myButton.colors;
            cb.normalColor = disabledColor;
            cb.highlightedColor = disabledColor;
            cb.pressedColor = disabledColor;
            cb.selectedColor = disabledColor;
            myButton.colors = cb;
        }
    }

    // Các slot khác
    public void slot2()
    {
        GoldManager.AddGold(10);
        lightningManeger.Spendlightning(1);
    } 
    public void slot3()
    {
        GoldManager.AddGold(20);
        lightningManeger.Spendlightning(2);
    }
    public void slot4()
    {
        GoldManager.AddGold(50);
        lightningManeger.Spendlightning(3);
    }
    public void slot5()
    {
        GoldManager.AddGold(100);
        lightningManeger.Spendlightning(5);
    }
    public void slot6()
    {
        GoldManager.AddGold(150);
        lightningManeger.Spendlightning(7);
    }
    public void slot7()
    {
        GoldManager.AddGold(300);
        lightningManeger.Spendlightning(10);
    }
    public void slot8()
    {
        GoldManager.AddGold(500);
        lightningManeger.Spendlightning(15);
    }
}
