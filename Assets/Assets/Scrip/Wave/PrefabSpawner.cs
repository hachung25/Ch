using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSpawner : MonoBehaviour
{
    public GameObject panelMode;
    public GameObject panelMode2;


    public void ShowPanelMode()
    {
        panelMode.SetActive(true);
    }
    
    public void loadScenemap()
    {
        SceneManager.LoadScene("Mode1");
        panelMode2.SetActive(false);
        panelMode.SetActive(false);
        
    } 

    
}

