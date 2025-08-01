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
        Debug.Log("Đã nhấn1");
        panelSpin.SetActive(true);
        panelRanking.SetActive(false);
    }
    public void ShowPanelRanking()
    {
        Debug.Log("Đã nhấn2");
        panelRanking.SetActive(true);
        panelSpin.SetActive(false);
    }
}
