using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class PlayerNameInit : MonoBehaviour
{
    const string KeyLastName = "last_display_name";

    void Awake()
    {
        // Nếu đã từng lưu tên trước đó thì set ngay -> UI/HUD dùng đúng tên lập tức
        var cached = PlayerPrefs.GetString(KeyLastName, "");
        if (!string.IsNullOrWhiteSpace(cached))
            PlayerName.Set(cached);

        DontDestroyOnLoad(gameObject);
    }

    public static void SaveCache(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        PlayerPrefs.SetString(KeyLastName, name.Trim());
        PlayerPrefs.Save();
    }
}
