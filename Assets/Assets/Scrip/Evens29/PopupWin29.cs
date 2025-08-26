using UnityEngine;

public class PopupWin29 : MonoBehaviour
{
    public GameObject popup;

    public void OnPopup()
    {
        popup.SetActive(true);
    }

    public void offPopup()
    {
        popup.SetActive(false);
    }
}
