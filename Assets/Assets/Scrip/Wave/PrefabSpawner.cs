using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSpawner : MonoBehaviour
{
    public GameObject panelMode;
  

    public void ShowPanelMode()
    {
        panelMode.SetActive(true);
    }
    
    public void loadScenemap()
    {
        SceneManager.LoadScene("Mode1");
        panelMode.SetActive(false);
      
        
    } 

    
}

