using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItemUI : MonoBehaviour
{
    public TMP_Text label;
    public Button btnJoin;

    string _roomId;
    Action<string> _onJoin;

    public void Setup(string roomId, int playerCount, Action<string> onJoin)
    {
        _roomId = roomId;
        _onJoin = onJoin;
        if (label) label.SetText($"{roomId} ({playerCount} players)");
        if (btnJoin)
        {
            btnJoin.onClick.RemoveAllListeners();
            btnJoin.onClick.AddListener(() => _onJoin?.Invoke(_roomId));
        }
    }
}
