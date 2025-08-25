using UnityEngine;

public class PanelOnoff : MonoBehaviour
{
    public GameObject panelSpin;
    public GameObject panelRanking;
    private void Start()
    {
        panelSpin.SetActive(true);
        panelRanking.SetActive(false);
    }

    public void ShowPanelSpin()
    {
        panelSpin.SetActive(true);
        panelRanking.SetActive(false);
    }
    public void ShowPanelRanking()
    {
       
        panelRanking.SetActive(true);
        panelSpin.SetActive(false);
    }
}
