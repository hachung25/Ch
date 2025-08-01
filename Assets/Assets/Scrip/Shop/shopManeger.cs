using UnityEngine;
using UnityEngine.UI;

public class shopManeger : MonoBehaviour
{
    
    public TB _Tb; 


    // Các slot khác
    public void slot2()
    {
        if (lightningManeger.GetLightning() >= 5)
        {
            GoldManager.AddGold(70);
            lightningManeger.SpendLightning(5);
            _Tb.ShowTbs();
        }
      
    } 
    public void slot3()
    {
        if (lightningManeger.GetLightning() >= 10)
        {
            GoldManager.AddGold(150);
            lightningManeger.SpendLightning(10);
            _Tb.ShowTbs();
        }
        
    }
    public void slot4()
    {
        if (lightningManeger.GetLightning() >= 30)
        {
             GoldManager.AddGold(500);
             lightningManeger.SpendLightning(30);
             _Tb.ShowTbs();
        }
       
    }
    public void slot5()
    {
        if (lightningManeger.GetLightning() >= 50)
        {
             GoldManager.AddGold(1000);
             lightningManeger.SpendLightning(50);
             _Tb.ShowTbs();
        }
       
    }
    public void slot6()
    {
        if (lightningManeger.GetLightning() >= 100)
        {
            GoldManager.AddGold(3000);
            lightningManeger.SpendLightning(100);  
            _Tb.ShowTbs();
        }
    }

    public void slot8()
    {
        if (lightningManeger.GetLightning() >= 20)
        {
            GoldManager.AddGold(300);
            lightningManeger.SpendLightning(20);
            _Tb.ShowTbs();
        }
    }

    public void sotCards1()
    {
        if (lightningManeger.GetLightning() >= 2)
        {
           CardsManeger.AddCards(1);
            lightningManeger.SpendLightning(2);
            _Tb.ShowTbs();
        }
    }
    public void sotCards2()
    {
        if (lightningManeger.GetLightning() >= 4)
        {
            CardsManeger.AddCards(2);
            lightningManeger.SpendLightning(4);
            _Tb.ShowTbs();
        }
    }

    public void slotEvents2th9()
    {
        if(lightningManeger.GetLightning()>= 30)
        {
            CardSpingManeger.AddCardSping(3);
            lightningManeger.SpendLightning(30);
            _Tb.ShowTbs();
        }
    }
}
