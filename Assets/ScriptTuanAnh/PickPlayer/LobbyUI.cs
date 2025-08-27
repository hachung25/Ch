using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("General")]
    public int gameplayBuildIndex = 2;

    [Header("Create / Join")]
    public TMP_InputField inputJoinRoomId;
    public Button btnCreateRoom;
    public Button btnJoinById;
    public Button btnStartAsHost;
    public Button btnDeleteRoom;
    public Button btnLeaveRoom;

    [Header("Room List (trái)")]
    public Transform roomListRoot;
    public GameObject roomItemPrefab;

    [Header("Players (phải)")]
    public Transform playerListRoot;
    public GameObject playerItemPrefab;
    public TMP_Text currentRoomLabel;
    public TMP_Text statusLabel;

    // ===== Unity =====
    async void OnEnable()
    {
        await CleanupZombieRooms();

        RoomService.I.StartRoomDirectory();
        RoomService.I.OnRoomListChanged += RefreshRoomList;
        RoomService.I.OnPlayersChanged += OnPlayersChanged;
        RoomService.I.OnHostChanged += OnHostChanged;
        RoomService.I.OnSceneLoadTriggered += OnSceneLoadTriggered;

        if (btnCreateRoom) btnCreateRoom.onClick.AddListener(OnClickCreateRoom);
        if (btnJoinById) btnJoinById.onClick.AddListener(OnClickJoinById);
        if (btnStartAsHost) btnStartAsHost.onClick.AddListener(OnClickStartAsHost);
        if (btnDeleteRoom) btnDeleteRoom.onClick.AddListener(OnClickDeleteRoom);
        if (btnLeaveRoom) btnLeaveRoom.onClick.AddListener(OnClickLeaveRoom);

        UpdateTopBar();
        UpdateButtons();
    }

    void OnDisable()
    {
        if (RoomService.I != null)
        {
            RoomService.I.OnRoomListChanged -= RefreshRoomList;
            RoomService.I.OnPlayersChanged -= OnPlayersChanged;
            RoomService.I.OnHostChanged -= OnHostChanged;
            RoomService.I.OnSceneLoadTriggered -= OnSceneLoadTriggered;
            RoomService.I.StopRoomDirectory();
        }
    }

    // ===== Buttons =====
    public async void OnClickCreateRoom()
    {
        try
        {
            var id = await RoomService.I.CreateRoomAndJoin();
            statusLabel?.SetText($"Tạo phòng & vào phòng: {id}");
            UpdateTopBar(); UpdateButtons();
        }
        catch (Exception e) { statusLabel?.SetText($"Create failed: {e.Message}"); }
    }

    public async void OnClickJoinById()
    {
        var id = inputJoinRoomId ? inputJoinRoomId.text?.Trim() : "";
        if (string.IsNullOrEmpty(id)) { statusLabel?.SetText("Nhập RoomId để join."); return; }

        try
        {
            await RoomService.I.JoinRoom(id);
            statusLabel?.SetText($"Đã vào phòng: {id}");
            UpdateTopBar(); UpdateButtons();
        }
        catch (Exception e) { statusLabel?.SetText($"Join failed: {e.Message}"); }
    }

    // Host bấm Start
    public void OnClickStartAsHost()
    {
        _ = HostStartGame(); // chạy game ngay, không countdown, không Canvas switch
    }

    private async Task HostStartGame()
    {
        try
        {
            await RoomService.I.HostTriggerStart(gameplayBuildIndex);
            statusLabel?.SetText("Game Started!");
        }
        catch (Exception e) { statusLabel?.SetText($"Start failed: {e.Message}"); }
    }

    public async void OnClickDeleteRoom()
    {
        try { await RoomService.I.DeleteRoom(); statusLabel?.SetText("Đã xoá phòng."); }
        catch (Exception e) { statusLabel?.SetText($"Delete failed: {e.Message}"); }

        UpdateTopBar(); UpdateButtons();
    }

    public async void OnClickLeaveRoom()
    {
        try { await RoomService.I.LeaveRoom(); statusLabel?.SetText("Đã rời phòng."); }
        catch (Exception e) { statusLabel?.SetText($"Leave failed: {e.Message}"); }

        UpdateTopBar(); UpdateButtons();
    }

    // ===== Event handlers =====
    void RefreshRoomList(List<(string roomId, RoomInfo info)> rooms)
    {
        if (!roomListRoot || !roomItemPrefab) return;

        foreach (Transform c in roomListRoot) Destroy(c.gameObject);

        foreach (var (roomId, info) in rooms)
        {
            var go = Instantiate(roomItemPrefab, roomListRoot);
            var item = go.GetComponent<RoomListItemUI>() ?? go.AddComponent<RoomListItemUI>();
            int count = info?.players != null ? info.players.Count : 0;

            item.Setup(roomId, count, async (id) =>
            {
                await RoomService.I.JoinRoom(id);
                statusLabel?.SetText($"Đã vào phòng: {id}");
                UpdateTopBar(); UpdateButtons();
            });
        }
    }

    void OnPlayersChanged(string roomId, Dictionary<string, PlayerInfo> players)
    {
        if (string.IsNullOrEmpty(roomId)) return;
        if (!playerListRoot || !playerItemPrefab) return;

        foreach (Transform c in playerListRoot) Destroy(c.gameObject);

        foreach (var kv in players)
        {
            var go = Instantiate(playerItemPrefab, playerListRoot);
            var ui = go.GetComponent<PlayerListItemUI>() ?? go.AddComponent<PlayerListItemUI>();
            string name = string.IsNullOrEmpty(kv.Value.name) ? kv.Key : kv.Value.name;
            ui.Setup(name, kv.Value.isHost, kv.Key);
        }

        UpdateTopBar();
        UpdateButtons();
    }

    void OnHostChanged(string newHostUid)
    {
        UpdateButtons();
    }

    void OnSceneLoadTriggered(int buildIndex, string roundId)
    {
        SceneManager.LoadScene(buildIndex);
    }

    // ===== Helpers =====
    void UpdateTopBar()
    {
        var id = RoomService.I.CurrentRoomId;
        currentRoomLabel?.SetText(string.IsNullOrEmpty(id) ? "Room: (none)" : $"Room: {id}");
    }

    void UpdateButtons()
    {
        bool inRoom = !string.IsNullOrEmpty(RoomService.I.CurrentRoomId);
        string myUid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        bool isHost = inRoom && RoomService.I.LastKnownHostUid == myUid;

        if (btnCreateRoom) btnCreateRoom.gameObject.SetActive(!inRoom);
        if (btnJoinById) btnJoinById.gameObject.SetActive(!inRoom);

        if (btnStartAsHost) btnStartAsHost.gameObject.SetActive(inRoom && isHost);
        if (btnDeleteRoom) btnDeleteRoom.gameObject.SetActive(inRoom && isHost);
        if (btnLeaveRoom) btnLeaveRoom.gameObject.SetActive(inRoom);
    }

    static async Task CleanupZombieRooms()
    {
        try
        {
            var root = await FirebaseDatabase.DefaultInstance.GetReference("rooms").GetValueAsync();
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var r in root.Children)
            {
                var players = r.Child("players");
                var expVal = r.Child("expiryAt").Value;
                long expiryAt = 0;
                if (expVal != null) long.TryParse(expVal.ToString(), out expiryAt);

                bool expired = expiryAt > 0 && now >= expiryAt;
                bool empty = !players.Exists || players.ChildrenCount == 0;

                if (expired || empty)
                {
                    try { await r.Reference.RemoveValueAsync(); } catch { }
                }
            }
        }
        catch { }
    }
}
