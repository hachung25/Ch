using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class LobbyUI : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Build Index của scene Gameplay trong Build Settings")]
    public int gameplayBuildIndex = 2;

    [Header("Create / Join")]
    public TMP_InputField inputJoinRoomId;
    public Button btnCreateRoom;
    public Button btnJoinById;
    public Button btnStartAsHost;
    public Button btnLeaveRoom;

    [Header("Room List UI")]
    public Transform roomListRoot;      // Content của ScrollView
    public GameObject roomItemPrefab;   // Prefab 1 dòng phòng (có TMP_Text + Button Join)

    [Header("Players in My Room")]
    public Transform playerListRoot;    // Content list player trong phòng mình
    public GameObject playerItemPrefab; // Prefab 1 dòng player (chỉ cần TMP_Text)
    public TMP_Text currentRoomLabel;   // "Room: <id>"
    public TMP_Text statusLabel;        // thông báo ngắn

    void OnEnable()
    {
        // Bắt đầu lắng nghe danh sách phòng
        RoomService.I.StartRoomDirectory();
        RoomService.I.OnRoomListChanged += RefreshRoomList;
        RoomService.I.OnPlayersChanged += OnPlayersChanged;

        // Nút bấm
        if (btnCreateRoom) btnCreateRoom.onClick.AddListener(OnClickCreateRoom);
        if (btnJoinById) btnJoinById.onClick.AddListener(OnClickJoinById);
        if (btnStartAsHost) btnStartAsHost.onClick.AddListener(OnClickStartAsHost);
        if (btnLeaveRoom) btnLeaveRoom.onClick.AddListener(OnClickLeaveRoom);

        UpdateTopBar();
    }

    void OnDisable()
    {
        if (RoomService.I != null)
        {
            RoomService.I.OnRoomListChanged -= RefreshRoomList;
            RoomService.I.OnPlayersChanged -= OnPlayersChanged;
        }
        if (btnCreateRoom) btnCreateRoom.onClick.RemoveListener(OnClickCreateRoom);
        if (btnJoinById) btnJoinById.onClick.RemoveListener(OnClickJoinById);
        if (btnStartAsHost) btnStartAsHost.onClick.RemoveListener(OnClickStartAsHost);
        if (btnLeaveRoom) btnLeaveRoom.onClick.RemoveListener(OnClickLeaveRoom);
    }

    // ===== Buttons =====
    public async void OnClickCreateRoom()
    {
        try
        {
            var id = await RoomService.I.CreateRoom(gameplayBuildIndex);
            statusLabel?.SetText($"Created room: {id}");
            UpdateTopBar();
        }
        catch (System.SystemException e)
        {
            statusLabel?.SetText($"Create failed: {e.Message}");
        }
    }

    public async void OnClickJoinById()
    {
        var id = inputJoinRoomId ? inputJoinRoomId.text?.Trim() : "";
        if (string.IsNullOrEmpty(id)) { statusLabel?.SetText("Nhập RoomId để join."); return; }

        try
        {
            await RoomService.I.JoinRoom(id);
            statusLabel?.SetText($"Joined: {id}");
            UpdateTopBar();
        }
        catch (System.SystemException e)
        {
            statusLabel?.SetText($"Join failed: {e.Message}");
        }
    }

    public async void OnClickStartAsHost()
    {
        await RoomService.I.HostTriggerStart();
    }

    public async void OnClickLeaveRoom()
    {
        await RoomService.I.LeaveRoom();
        ClearPlayerList();
        UpdateTopBar();
    }

    // ===== Event handlers =====
    void RefreshRoomList(List<(string roomId, RoomInfo info)> rooms)
    {
        // Clear
        foreach (Transform c in roomListRoot) Destroy(c.gameObject);

        // Fill
        foreach (var (roomId, info) in rooms)
        {
            var go = Instantiate(roomItemPrefab, roomListRoot);
            var item = go.GetComponent<RoomListItemUI>();
            if (item == null) item = go.AddComponent<RoomListItemUI>();
            int count = info?.players != null ? info.players.Count : 0;
            item.Setup(roomId, count, async (id) =>
            {
                await RoomService.I.JoinRoom(id);
                statusLabel?.SetText($"Joined: {id}");
                UpdateTopBar();
            });
        }
    }

    void OnPlayersChanged(string roomId, Dictionary<string, PlayerInfo> players)
    {
        // Chỉ render danh sách nếu là phòng mình đang ở
        if (roomId != RoomService.I.CurrentRoomId) return;

        // Clear
        ClearPlayerList();

        // Fill
        foreach (var kv in players)
        {
            var go = Instantiate(playerItemPrefab, playerListRoot);
            var ui = go.GetComponent<PlayerListItemUI>();
            if (ui == null) ui = go.AddComponent<PlayerListItemUI>();

            string name = string.IsNullOrEmpty(kv.Value.name) ? kv.Key : kv.Value.name;
            bool isHost = kv.Value.isHost;
            ui.Setup(name, isHost);
        }

        // Bật/tắt nút Start theo quyền Host
        btnStartAsHost?.gameObject.SetActive(IsMeHost(players));
        UpdateTopBar();
    }

    // ===== Helpers =====
    void UpdateTopBar()
    {
        var id = RoomService.I.CurrentRoomId;
        currentRoomLabel?.SetText(string.IsNullOrEmpty(id) ? "Room: (none)" : $"Room: {id}");
    }

    void ClearPlayerList()
    {
        foreach (Transform c in playerListRoot) Destroy(c.gameObject);
        btnStartAsHost?.gameObject.SetActive(false);
    }

    bool IsMeHost(Dictionary<string, PlayerInfo> players)
    {
        var uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid) || players == null) return false;
        return players.TryGetValue(uid, out var me) && me.isHost;
    }
}
