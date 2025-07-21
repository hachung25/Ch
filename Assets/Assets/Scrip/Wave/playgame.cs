using UnityEngine;

public class playgame : MonoBehaviour
{
  public GameObject Mainmenu;
  public GameObject PauseMenu;
  public GameObject panelPause;

  public void Playgame()
  {
    PauseMenu.SetActive(true);
    Mainmenu.SetActive(false);
    if (panelPause.activeInHierarchy)
    {
       Time.timeScale = 0;
    }
    
  }
  
}
