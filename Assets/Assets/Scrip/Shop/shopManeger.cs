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
        if (lightningManeger.GetLightning() >= 1)
        {
            GoldManager.AddGold(10);
            lightningManeger.SpendLightning(1);
        }
      
    } 
    public void slot3()
    {
        if (lightningManeger.GetLightning() >= 2)
        {
            GoldManager.AddGold(20);
            lightningManeger.SpendLightning(2);
        }
        
    }
    public void slot4()
    {
        if (lightningManeger.GetLightning() >= 3)
        {
             GoldManager.AddGold(50);
             lightningManeger.SpendLightning(3);
        }
       
    }
    public void slot5()
    {
        if (lightningManeger.GetLightning() >= 5)
        {
             GoldManager.AddGold(100);
             lightningManeger.SpendLightning(5);
        }
       
    }
    public void slot6()
    {
        if (lightningManeger.GetLightning() >= 7)
        {
            GoldManager.AddGold(150);
            lightningManeger.SpendLightning(7);  
        }
    }
    public void slot7()
    {
        if (lightningManeger.GetLightning() >= 10)
        {
            GoldManager.AddGold(300);
            lightningManeger.SpendLightning(10);
        }
    }
    public void slot8()
    {
        if (lightningManeger.GetLightning() >= 15)
        {
            GoldManager.AddGold(500);
            lightningManeger.SpendLightning(15);
        }
    }
}
