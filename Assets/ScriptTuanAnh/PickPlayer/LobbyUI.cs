using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("General")]
    public int gameplayBuildIndex = 2;

    [Header("Countdown Settings")]
    [Tooltip("Thời gian đếm ngược khi host bấm Start (giây)")]
    public int hostCountdownSeconds = 10;   // 👈 chỉnh được trong Inspector

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

    [Header("Countdowns")]
    public TMP_Text countdownLabel;               // đếm auto-delete (CanvasRoom)
    public TMP_Text startHostCountdownLabel;      // đếm khi host bấm Start (CanvasPlayer)

    [Header("Canvas chuyển đổi")]
    public GameObject CanvasRoom;
    public GameObject CanvasPlayer;

    // State
    private bool _expiryCountdownRunning;
    private Coroutine _startHostCountdownRoutine;

    // Firebase status listener (để client đổi UI khi host bấm Start)
    private DatabaseReference _statusRef;
    private EventHandler<ValueChangedEventArgs> _statusHandler;

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

        ClearStartHostLabel();
        ClearExpiryLabel();

        _expiryCountdownRunning = false;
        _startHostCountdownRoutine = null;
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

        if (btnCreateRoom) btnCreateRoom.onClick.RemoveListener(OnClickCreateRoom);
        if (btnJoinById) btnJoinById.onClick.RemoveListener(OnClickJoinById);
        if (btnStartAsHost) btnStartAsHost.onClick.RemoveListener(OnClickStartAsHost);
        if (btnDeleteRoom) btnDeleteRoom.onClick.RemoveListener(OnClickDeleteRoom);
        if (btnLeaveRoom) btnLeaveRoom.onClick.RemoveListener(OnClickLeaveRoom);

        DetachStatusListener();
        StopExpiryCountdown();
        StopStartHostCountdown();
    }

    // ===== Buttons =====
    public async void OnClickCreateRoom()
    {
        try
        {
            var id = await RoomService.I.CreateRoomAndJoin();
            statusLabel?.SetText($"Tạo phòng & vào phòng: {id}");
            UpdateTopBar();
            UpdateButtons();

            await ForceRefreshPlayersUI(id);
            _ = BeginCountdownByExpiry(id);
        }
        catch (Exception e)
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
            statusLabel?.SetText($"Đã vào phòng: {id}");
            UpdateTopBar();
            UpdateButtons();

            await ForceRefreshPlayersUI(id);
            _ = BeginCountdownByExpiry(id);
        }
        catch (Exception e) { statusLabel?.SetText($"Join failed: {e.Message}"); }
    }

    // Host bấm Start
    public async void OnClickStartAsHost()
    {
        try
        {
            var roomId = RoomService.I.CurrentRoomId;
            if (!string.IsNullOrEmpty(roomId))
            {
                await FirebaseDatabase.DefaultInstance
                    .GetReference("rooms").Child(roomId).Child("status")
                    .SetValueAsync("prestart");
            }
        }
        catch { }

        if (CanvasRoom) CanvasRoom.SetActive(false);
        if (CanvasPlayer) CanvasPlayer.SetActive(true);

        StopExpiryCountdown();
        StopStartHostCountdown();

        // dùng biến inspector thay vì fix 30
        _startHostCountdownRoutine = StartCoroutine(StartHostCountdown(hostCountdownSeconds));
    }

    private IEnumerator StartHostCountdown(int seconds)
    {
        int remain = seconds;
        while (remain > 0)
        {
            if (startHostCountdownLabel)
                startHostCountdownLabel.SetText($"Game will start in {remain}s");

            yield return new WaitForSeconds(1f);
            remain--;
        }

        ClearStartHostLabel();
        _startHostCountdownRoutine = null;

        _ = HostStartGame();
    }

    private async Task HostStartGame()
    {
        try
        {
            int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
            Debug.Log("Game starting, my character index = " + selectedIndex);

            await RoomService.I.HostTriggerStart(gameplayBuildIndex);
            statusLabel?.SetText("Game Started!");
        }
        catch (Exception e)
        {
            statusLabel?.SetText($"Start failed: {e.Message}");
        }
    }

    public async void OnClickDeleteRoom()
    {
        try
        {
            await RoomService.I.DeleteRoom();
            statusLabel?.SetText("Đã xoá phòng.");
        }
        catch (Exception e)
        {
            statusLabel?.SetText($"Delete failed: {e.Message}");
        }

        DetachStatusListener();
        ClearPlayerList();
        UpdateTopBar();
        UpdateButtons();

        StopExpiryCountdown();
        StopStartHostCountdown();
    }

    public async void OnClickLeaveRoom()
    {
        try { await RoomService.I.LeaveRoom(); statusLabel?.SetText("Đã rời phòng."); }
        catch (Exception e) { statusLabel?.SetText($"Leave failed: {e.Message}"); }

        DetachStatusListener();
        ClearPlayerList();
        UpdateTopBar();
        UpdateButtons();

        StopExpiryCountdown();
        StopStartHostCountdown();
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
                UpdateTopBar();
                UpdateButtons();
                await ForceRefreshPlayersUI(id);
                _ = BeginCountdownByExpiry(id);
            });
        }
    }

    void OnPlayersChanged(string roomId, Dictionary<string, PlayerInfo> players)
    {
        if (string.IsNullOrEmpty(roomId) || players.Count == 0)
        {
            statusLabel?.SetText("Phòng đã bị xoá hoặc trống.");
            DetachStatusListener();
            ClearPlayerList();
            UpdateTopBar();
            UpdateButtons();
            StopExpiryCountdown();
            StopStartHostCountdown();
            return;
        }

        if (roomId != RoomService.I.CurrentRoomId) return;
        if (!playerListRoot || !playerItemPrefab) return;

        ClearPlayerList();

        var list = players?.ToList() ?? new List<KeyValuePair<string, PlayerInfo>>();
        list = list.OrderByDescending(kv => kv.Value.isHost)
                   .ThenBy(kv => kv.Value.joinedAt)
                   .ToList();

        foreach (var kv in list)
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
        _ = BeginCountdownByExpiry(RoomService.I.CurrentRoomId);
    }

    void OnSceneLoadTriggered(int sceneIndex, string roundId)
    {
        Debug.Log($"[LobbyUI] SceneLoadTriggered {sceneIndex}, round {roundId}");

        if (CanvasRoom) CanvasRoom.SetActive(false);
        if (CanvasPlayer) CanvasPlayer.SetActive(false);

        SceneManager.LoadScene(sceneIndex);
    }

    // ===== Helpers =====
    void UpdateTopBar()
    {
        var id = RoomService.I.CurrentRoomId;
        currentRoomLabel?.SetText(string.IsNullOrEmpty(id) ? "Room: (none)" : $"Room: {id}");
    }

    void ClearPlayerList()
    {
        if (!playerListRoot) return;
        foreach (Transform c in playerListRoot) Destroy(c.gameObject);
    }

    void UpdateButtons()
    {
        bool inRoom = !string.IsNullOrEmpty(RoomService.I.CurrentRoomId);
        string myUid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        bool isHost = inRoom && !string.IsNullOrEmpty(RoomService.I.LastKnownHostUid) &&
                      RoomService.I.LastKnownHostUid == myUid;

        if (btnCreateRoom) btnCreateRoom.gameObject.SetActive(!inRoom);
        if (btnJoinById) btnJoinById.gameObject.SetActive(!inRoom);

        if (btnStartAsHost) btnStartAsHost.gameObject.SetActive(inRoom && isHost);
        if (btnDeleteRoom) btnDeleteRoom.gameObject.SetActive(inRoom && isHost);
        if (btnLeaveRoom) btnLeaveRoom.gameObject.SetActive(inRoom);

        if (!isHost) StopExpiryCountdown();
    }

    async Task ForceRefreshPlayersUI(string roomId)
    {
        try
        {
            var playersSnap = await FirebaseDatabase.DefaultInstance
                .GetReference("rooms").Child(roomId).Child("players").GetValueAsync();

            var dict = new Dictionary<string, PlayerInfo>();
            foreach (var ch in playersSnap.Children)
            {
                var json = ch.GetRawJsonValue();
                if (!string.IsNullOrEmpty(json))
                    dict[ch.Key] = JsonUtility.FromJson<PlayerInfo>(json);
            }
            OnPlayersChanged(roomId, dict);

            AttachStatusListener(roomId);
        }
        catch { }
    }

    async Task BeginCountdownByExpiry(string roomId)
    {
        if (string.IsNullOrEmpty(roomId)) { StopExpiryCountdown(); return; }

        bool isHost = RoomService.I.LastKnownHostUid == FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (!isHost) { StopExpiryCountdown(); return; }

        var snap = await FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomId).Child("expiryAt").GetValueAsync();
        if (!snap.Exists) { StopExpiryCountdown(); return; }

        long expiryAt = 0; long.TryParse(snap.Value.ToString(), out expiryAt);
        if (expiryAt <= 0) { StopExpiryCountdown(); return; }

        _expiryCountdownRunning = true;

        while (_expiryCountdownRunning)
        {
            isHost = RoomService.I.LastKnownHostUid == FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
            if (!isHost) { StopExpiryCountdown(); break; }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long remain = expiryAt - now;
            if (remain <= 0)
            {
                StopExpiryCountdown();
                break;
            }

            if (countdownLabel)
            {
                int m = (int)(remain / 1000) / 60;
                int s = (int)(remain / 1000) % 60;
                countdownLabel.SetText($"Auto delete in {m:D2}:{s:D2}");
            }
            await Task.Delay(1000);
        }
    }

    void StopExpiryCountdown()
    {
        _expiryCountdownRunning = false;
        ClearExpiryLabel();
    }

    void ClearExpiryLabel()
    {
        if (countdownLabel) countdownLabel.SetText("");
    }

    void StopStartHostCountdown()
    {
        if (_startHostCountdownRoutine != null)
        {
            StopCoroutine(_startHostCountdownRoutine);
            _startHostCountdownRoutine = null;
        }
        ClearStartHostLabel();
    }

    void ClearStartHostLabel()
    {
        if (startHostCountdownLabel) startHostCountdownLabel.SetText("");
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

    void AttachStatusListener(string roomId)
    {
        DetachStatusListener();
        if (string.IsNullOrEmpty(roomId)) return;

        _statusRef = FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomId).Child("status");

        _statusHandler = (s, e) =>
        {
            var val = e.Snapshot?.Value?.ToString();
            if (!string.IsNullOrEmpty(val))
            {
                Debug.Log("[LobbyUI] room status = " + val);
                if (val == "prestart")
                {
                    if (CanvasRoom) CanvasRoom.SetActive(false);
                    if (CanvasPlayer) CanvasPlayer.SetActive(true);
                }
            }
        };
        _statusRef.ValueChanged += _statusHandler;
    }

    void DetachStatusListener()
    {
        if (_statusRef != null && _statusHandler != null)
        {
            _statusRef.ValueChanged -= _statusHandler;
            _statusHandler = null;
            _statusRef = null;
        }
    }
}