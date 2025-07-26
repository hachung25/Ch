using UnityEngine;

public class panelUpdate : MonoBehaviour
{
    public GameObject panelDame;
    public GameObject panelheath;

    void start()
    {
        panelheath.SetActive(true);
    }
    public void PanelUpdateDame()
    {
     panelDame.SetActive(true);
     panelheath.SetActive(false);
    } 
    public void PanelUpdateHeath()
    {
        panelheath.SetActive(true);
        panelDame.SetActive(false);
    }
}
