using TMPro;
using UnityEngine;

public class PlayerListItemUI : MonoBehaviour
{
    public TMP_Text label;

    public void Setup(string displayName, bool isHost)
    {
        if (label == null) return;
        label.SetText(isHost ? $"{displayName}  (Host)" : displayName);
    }
}
