using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow : MonoBehaviour
{
    [SerializeField] private RoomService room;

    void Awake()
    {
        if (!room) room = RoomService.I;
    }

    void OnEnable()
    {
        if (room) room.OnSceneLoadTriggered += HandleStartGame;
    }

    void OnDisable()
    {
        if (room) room.OnSceneLoadTriggered -= HandleStartGame;
    }

    void HandleStartGame(int buildIndex)
    {
        // Khi host bật trigger, toàn bộ máy đang lobby sang Gameplay
        SceneManager.LoadScene(buildIndex);
    }
}
