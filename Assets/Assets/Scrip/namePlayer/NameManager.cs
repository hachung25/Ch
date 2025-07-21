using UnityEngine;
using TMPro; // nếu dùng TextMeshPro

public class NameManager : MonoBehaviour
{
    public TMP_InputField nameInputField; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI nameTextUpdate;
    private const string PlayerNameKey = "PlayerName";

    public void SaveName()
    {
        string playerName = nameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString(PlayerNameKey, playerName);
            PlayerPrefs.Save();
            Debug.Log("Đã lưu tên: " + playerName);
            UpdateName();
        }
    }

    void Start()
    {
        // Tự động load tên nếu có
        if (PlayerPrefs.HasKey(PlayerNameKey))
        {
            nameInputField.text = PlayerPrefs.GetString(PlayerNameKey);
            nameText.text = PlayerPrefs.GetString(PlayerNameKey);
            nameTextUpdate.text = PlayerPrefs.GetString(PlayerNameKey);
        }
    }

    public void UpdateName()
    {
        nameInputField.text = PlayerPrefs.GetString(PlayerNameKey);
        nameText.text = PlayerPrefs.GetString(PlayerNameKey);
        nameTextUpdate.text = PlayerPrefs.GetString(PlayerNameKey);
    }
}