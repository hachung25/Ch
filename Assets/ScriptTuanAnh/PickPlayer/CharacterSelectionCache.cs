using UnityEngine;

public static class CharacterSelectionCache
{
    const string Key = "char_selected_id";
    public static int SelectedId { get; private set; } = -1;

    public static void Set(int id)
    {
        SelectedId = id;
        PlayerPrefs.SetInt(Key, id);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        SelectedId = PlayerPrefs.GetInt(Key, -1);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() => Load();
}
