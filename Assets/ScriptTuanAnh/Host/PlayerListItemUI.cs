using TMPro;
using UnityEngine;

public class PlayerListItemUI : MonoBehaviour
{
    public TMP_Text label;

    public void Setup(string displayName, bool isHost, string uid = null)
    {
        if (!label) return;

        // Nếu không có tên thì fallback về uid
        string showName = string.IsNullOrEmpty(displayName) ? uid ?? "Unknown" : displayName;

        if (isHost)
            label.SetText($"{showName}  (Host)");
        else
            label.SetText(showName);
    }
}
