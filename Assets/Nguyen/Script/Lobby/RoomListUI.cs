using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class RoomListUI : MonoBehaviour
{
    public GameObject roomItemPrefab;
    public Transform contentParent;
    public LobbyManager lobbyManager;

    public void UpdateRoomList(List<SessionInfo> sessions)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var session in sessions)
        {
            var item = Instantiate(roomItemPrefab, contentParent);
            var roomItem = item.GetComponent<RoomListItem>();
            roomItem.SetUp(session, () =>
            {
                lobbyManager.JoinRoom(session.Name);
            });
        }
    }
}
