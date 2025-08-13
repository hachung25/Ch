using System;
using UnityEngine;
using TMPro;

public class goldPause : MonoBehaviour
{
   public MapManager mapManager;
   public TextMeshProUGUI textlose;

    void OnEnable()
    {
        int gold;
        gold = mapManager.GoldWave;
        Debug.Log(gold);
        textlose.text = gold.ToString();
        GoldManager.AddGold(gold);
    }
}
