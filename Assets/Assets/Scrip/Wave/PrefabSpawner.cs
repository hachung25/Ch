using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Map của từng mode")]
    public GameObject[] mode1Maps;
    public GameObject[] mode2Maps;
    public GameObject[] mode3Maps;

    public Transform spawnPoint;

    private GameObject currentMap;
    private int currentModeIndex;
    private int currentMapIndex;
    private GameObject[][] allModes;
    private GameObject[][] allMapInstances; // chứa tất cả map đã được clone
    private bool isPaused = false;
    public GameObject homePanel; 

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
                GameObject mapInstance = Instantiate(allModes[mode][map], spawnPoint.position, Quaternion.identity);
                mapInstance.SetActive(false); // Ẩn ban đầu
                allMapInstances[mode][map] = mapInstance;
            }
        }

        // Load tiến trình
        currentModeIndex = PlayerPrefs.GetInt("CurrentModeIndex", 0);
        currentMapIndex = PlayerPrefs.GetInt("CurrentMapIndex", 0);

        // KHÔNG gọi ShowMap tại đây nữa
        // => Map sẽ không hiển thị cho đến khi nhấn PlayGame()

        if (homePanel != null)
            homePanel.SetActive(true); // Hiện giao diện Home
    }


    public void ShowMap(int modeIndex, int mapIndex)
    {
        // Ẩn map hiện tại nếu có
        if (currentMap != null)
            currentMap.SetActive(false);

        // Hiện map mới
        currentMap = allMapInstances[modeIndex][mapIndex];
        currentMap.SetActive(true);

        // Cập nhật và lưu tiến trình
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
            currentMap.SetActive(false); // Ẩn map nếu đang hiện

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
        // Ẩn map hiện tại
        if (currentMap != null)
            currentMap.SetActive(false);

        // Hiện giao diện Home
        if (homePanel != null)
            homePanel.SetActive(true);

        // Nếu có pause trước đó thì resume lại
        Time.timeScale = 1f;

        Debug.Log("Đã trở về màn hình Home.");
    }
    public void PlayGame()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        // Hiển thị lại map hiện tại
        ShowMap(currentModeIndex, currentMapIndex);
    }

}
