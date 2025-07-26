using UnityEngine;
using UnityEngine.SceneManagement;

public class playgame : MonoBehaviour
{
  public GameObject Mainmenu;
  public GameObject PauseMenu;
  public GameObject panelPause;
  public string sceneToLoad = "Mode1";
    public void Playgame()
    {
    PauseMenu.SetActive(true);
    Mainmenu.SetActive(false);
    if (panelPause.activeInHierarchy)
    {
       Time.timeScale = 0;
    }
    
    }

  public void ExitGamePlay()
  {
      PauseMenu.SetActive(false);
  }

    public void OnPlayButtonOnGame()
    {
        SceneManager.LoadScene(sceneToLoad); 
    }
}
