using UnityEngine;

public class playmode : MonoBehaviour
{
   public GameObject playMode;
 

   public void Playmode()
   {
      playMode.SetActive(true);
   }

   public void ExitMode()
   {
      playMode.SetActive(false);
   }
}
