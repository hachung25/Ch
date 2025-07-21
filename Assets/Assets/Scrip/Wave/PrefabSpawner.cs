using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Map của từng mode")]
    public GameObject[] mode1Maps;
    public GameObject[] mode2Maps;
    public GameObject[] mode3Maps;

    public Transform spawnPoint;
    public Transform waveParent; 

    private GameObject currentMap;
    private int currentModeIndex;
    private int currentMapIndex;
    private GameObject[][] allModes;
    private GameObject[][] allMapInstances;
    private bool isPaused = false;
    public GameObject homePanel;
    public GameObject PanelPlaymode;

    void Start()
    {
        // Gom các mode thành mảng 2 chiều
        allModes = new GameObject[][]
        {
            mode1Maps,
            mode2Maps,
            mode3Maps
        };

        // Khởi tạo mảng chứa các instance đã clone
        allMapInstances = new GameObject[allModes.Length][];
        for (int mode = 0; mode < allModes.Length; mode++)
        {
            allMapInstances[mode] = new GameObject[allModes[mode].Length];

            for (int map = 0; map < allModes[mode].Length; map++)
            {
                GameObject mapInstance = Instantiate(
                    allModes[mode][map],
                    spawnPoint.position,
                    Quaternion.identity,
                    waveParent 
                );

                mapInstance.SetActive(false); 
                allMapInstances[mode][map] = mapInstance;
            }
        }


        currentModeIndex = PlayerPrefs.GetInt("CurrentModeIndex", 0);
        currentMapIndex = PlayerPrefs.GetInt("CurrentMapIndex", 0);

        if (homePanel != null)
            homePanel.SetActive(true); 
    }

    public void ShowMap(int modeIndex, int mapIndex)
    {
        if (currentMap != null)
            currentMap.SetActive(false);

        currentMap = allMapInstances[modeIndex][mapIndex];
        currentMap.SetActive(true);

        currentModeIndex = modeIndex;
        currentMapIndex = mapIndex;
        PlayerPrefs.SetInt("CurrentModeIndex", currentModeIndex);
        PlayerPrefs.SetInt("CurrentMapIndex", currentMapIndex);
        PlayerPrefs.Save();

        Debug.Log($"Đang chơi: Mode {modeIndex + 1}, Map {mapIndex + 1}");
    }

    public void NextMap()
    {
        currentMapIndex++;

        if (currentMapIndex >= allMapInstances[currentModeIndex].Length)
        {
            currentModeIndex++;
            currentMapIndex = 0;

            if (currentModeIndex >= allMapInstances.Length)
            {
                Debug.Log("Đã chơi hết tất cả các mode!");
                return;
            }
        }

        ShowMap(currentModeIndex, currentMapIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextMap();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("CurrentModeIndex");
        PlayerPrefs.DeleteKey("CurrentMapIndex");

        currentModeIndex = 0;
        currentMapIndex = 0;

        if (currentMap != null)
            currentMap.SetActive(false);

        Debug.Log("Đã ResetProgress về Map 1 - Mode 1");
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    public void BackToHome()
    {
        if (currentMap != null)
            currentMap.SetActive(false);

        if (homePanel != null)
            homePanel.SetActive(true);
        PanelPlaymode.SetActive(false);

        Time.timeScale = 1f;
        Debug.Log("Đã trở về màn hình Home.");
    }

    public void PlayGame()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        ShowMap(currentModeIndex, currentMapIndex);
    }
}
