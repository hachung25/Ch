using UnityEngine;

public class playmode : MonoBehaviour
{
   public GameObject playMode; 
   public GameObject CanvasCreatePlayer;
   public TB Thongbao;
   public void Playmode()
   {
      playMode.SetActive(true);
        CanvasCreatePlayer.SetActive(false);

    }

   public void ExitMode()
   {
      playMode.SetActive(false);

        
   }

    public void canvasCreatePlayer()
    {
       int amount = CardsManeger.GetCards();
       if (amount > 0)
       {
           Mutiplayer();
           CardsManeger.SpendCards(1);
       }
       else
       {
           Thongbao.showTb();
       }
    }

    public void Mutiplayer()
    {
        CanvasCreatePlayer.SetActive(true);
        playMode.SetActive(false);
    }
}
