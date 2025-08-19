using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow : MonoBehaviour
{
    [SerializeField] private RoomService room;

    // Tránh xử lý trùng cùng 1 roundId
    private readonly HashSet<string> _processedRounds = new();

    void Awake()
    {
        if (!room) room = RoomService.I;
    }

    void OnEnable()
    {
        if (room != null) room.OnSceneLoadTriggered += HandleStartGame;
    }

    void OnDisable()
    {
        if (room != null) room.OnSceneLoadTriggered -= HandleStartGame;
    }

    void HandleStartGame(int buildIndex, string roundId)
    {
        if (buildIndex < 0) return;
        if (!string.IsNullOrEmpty(roundId))
        {
            if (_processedRounds.Contains(roundId)) return;
            _processedRounds.Add(roundId);
        }
        SceneManager.LoadScene(buildIndex);
    }
}
