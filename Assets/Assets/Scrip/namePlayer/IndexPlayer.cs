using System;
using UnityEngine;
using TMPro;

public class IndexPlayer : MonoBehaviour
{
    public TextMeshProUGUI text1heath;
    public TextMeshProUGUI text2Dame;

    private int heath;
    private int Dame;

    public void updateIndex()
    {
        heath=PlayerPrefs.GetInt("Upgrade_Health");
        Dame=PlayerPrefs.GetInt("Upgrade_Damage");
        text1heath.text=heath.ToString();
        text2Dame.text=Dame.ToString();
    }
    
}
