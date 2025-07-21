using UnityEngine;

public class playmode : MonoBehaviour
{
   public GameObject playMode;

    public GameObject CanvasCreatePlayer;
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
        CanvasCreatePlayer.SetActive(true);
        playMode.SetActive(false);
    }
}
