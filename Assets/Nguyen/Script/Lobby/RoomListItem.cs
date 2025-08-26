using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class RoomListItem : MonoBehaviour
{
    public Text roomNameText;
    public Button joinButton;

    public void SetUp(SessionInfo session, System.Action onJoin)
    {
        roomNameText.text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onJoin.Invoke());
    }
}
