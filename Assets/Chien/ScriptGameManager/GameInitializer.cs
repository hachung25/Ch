using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.transform.position = PlayerData.Instance.defaultPosition;
            PlayerData.Instance.ResetPlayer(); // reset máu, animation, v.v.
        }
    }

}
