using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PanelInfo
{
    public string id;
    public GameObject panel;
}

public class PanelMain : MonoBehaviour
{
    [Header("Danh sách các Panel (ID + GameObject)")]
    public List<PanelInfo> panelInfos;

    private Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Chuyển danh sách sang Dictionary để truy xuất nhanh
        foreach (var info in panelInfos)
        {
            if (!panelDict.ContainsKey(info.id) && info.panel != null)
            {
                panelDict.Add(info.id, info.panel);
            }
        }
    }

    // Bật panel theo ID
    public void ShowPanel(string id)
    {
        foreach (var kvp in panelDict)
        {
            kvp.Value.SetActive(kvp.Key == id);
        }
    }

    // Tắt toàn bộ panel
    public void HideAllPanels()
    {
        foreach (var panel in panelDict.Values)
        {
            panel.SetActive(false);
        }
    }
}