using System;
using UnityEngine;
using TMPro;

public class DameHeath : MonoBehaviour
{
    public TextMeshProUGUI text2Dame;  
    public TextMeshProUGUI text1heath;
    private int heath;
    private int Dame;

     void Start()
    {
        Dame=PlayerPrefs.GetInt("Upgrade_Damage");
        heath=PlayerPrefs.GetInt("Upgrade_Health");
        text1heath.text=heath.ToString();
        text2Dame.text = Dame.ToString();
    }

    public void UpdateDameHeath()
    {
        Dame=PlayerPrefs.GetInt("Upgrade_Damage");
        heath=PlayerPrefs.GetInt("Upgrade_Health");
        text1heath.text=heath.ToString();
        text2Dame.text = Dame.ToString();
    }
}
