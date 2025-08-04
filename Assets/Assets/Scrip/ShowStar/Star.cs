using System;
using UnityEngine;

public class Star : MonoBehaviour
{
    public GameObject PanelStar;

    private void Start()
    {
        PanelStar.SetActive(true);
    }

    public void Ofpanel()
    {
        PanelStar.SetActive(false);
    }
}
