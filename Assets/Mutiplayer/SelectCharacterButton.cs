using UnityEngine;
using UnityEngine.UI; // Cần thêm thư viện này

public class SelectCharacterButton : MonoBehaviour
{
    public int characterIndex; // Gán chỉ số nhân vật trong Inspector (0, 1, 2...)
    

    public void OnCharacterSelected()
    {
        // Gán chỉ số nhân vật đã chọn vào CharacterSelectionManager
        if (CharacterSelectionManager.Instance != null)
        {
            CharacterSelectionManager.Instance.selectedCharacterIndex = characterIndex;
        }
    }
}