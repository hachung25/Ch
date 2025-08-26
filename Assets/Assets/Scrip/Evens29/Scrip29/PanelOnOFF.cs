using UnityEngine;

public class PanelOnOFF : MonoBehaviour
{
   public GameObject panel;

    public void ShowPanel()
    {
        panel.SetActive(true);
    }
}
