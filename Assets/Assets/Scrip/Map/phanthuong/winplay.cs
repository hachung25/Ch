using UnityEngine;
using TMPro;

public class winplay : MonoBehaviour
{
    public MapManager mapManager;
    public TextMeshProUGUI Goldtex; 
    public TextMeshProUGUI Gemtext;

    void OnEnable()
    {
        int gold;
        int gem;
        gold = mapManager.GoldWave;
        gem = mapManager.GemWave;
        Goldtex.text = gold.ToString();
        Gemtext.text = gem.ToString();
        lightningManeger.AddLightning(gem);
        GoldManager.AddGold(gold);
    }
}
