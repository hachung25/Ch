using UnityEngine;

public class ChestButton : MonoBehaviour
{
    public GameObject targetObject; // đối tượng cần bật/tắt
    private bool isOn = false;

    public void Toggle()
    {
        isOn = !isOn;
        targetObject.SetActive(isOn);
    }
}
