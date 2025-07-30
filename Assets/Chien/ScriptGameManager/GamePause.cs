using UnityEngine;

public class GamePause : MonoBehaviour
{
    public GameObject PauseMain;
   public void loadPause()
    {
        PauseMain.SetActive(true);
    }
}
