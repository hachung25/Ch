using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow : MonoBehaviour
{
    void OnEnable()
    {
        RoomService.I.OnSceneLoadTriggered += LoadAll;
        RoomService.I.StartRoomDirectory(); // để in danh sách phòng ngoài Lobby
    }
    void OnDisable()
    {
        if (RoomService.I != null)
            RoomService.I.OnSceneLoadTriggered -= LoadAll;
    }
    void LoadAll(int buildIndex) => SceneManager.LoadScene(buildIndex);
}
