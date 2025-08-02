using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    // Sử dụng một biến để lưu trữ chỉ số của nhân vật đã chọn.
    // 0 cho nhân vật đầu tiên, 1 cho nhân vật thứ hai, v.v.
    public int selectedCharacterIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Dòng này đảm bảo đối tượng không bị hủy khi chuyển scene.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu đã có một Instance, hủy đối tượng này đi.
            Destroy(gameObject);
        }
    }
}