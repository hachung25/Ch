using System.Collections;
using UnityEngine;

public class RainOfBulletsSkill : MonoBehaviour
{
    private const string PrefKey = "RainSkillUnlocked";

    [Header("Unlock State")]
    [Tooltip("Sẽ bị override bởi PlayerPrefs vào Awake()")]
    public bool isUnlocked = false;

    [Header("Key & Cooldown")]
    public KeyCode castKey = KeyCode.K;
    public float cooldownSeconds = 10f;

    [Header("Projectile")]
    public BulletRainProjectile projectilePrefab; // kéo prefab đạn vào đây

    [Tooltip("Số đạn ngẫu nhiên: [min, max]")]
    [Min(1)] public int minProjectiles = 1;
    [Min(1)] public int maxProjectiles = 20;

    [Header("Spawn Area / Pattern")]
    [Tooltip("Bật để rải phủ toàn bề ngang camera; tắt để rải quanh player")]
    public bool coverWholeCameraWidth = false;

    [Tooltip("Độ cao spawn so với player hoặc mép trên camera")]
    public float spawnHeight = 10f;

    [Tooltip("Biên độ ngang quanh player (khi không phủ camera)")]
    public float horizontalRange = 10f;

    [Tooltip("Rung ngẫu nhiên theo trục Y để mỗi viên đạn không cùng 1 đường thẳng")]
    public float yJitter = 1.5f;

    [Header("Timing (mỗi viên spawn lệch nhau)")]
    [Tooltip("Khoảng delay tối thiểu giữa 2 viên")]
    public float perBulletDelayMin = 0.03f;
    [Tooltip("Khoảng delay tối đa giữa 2 viên")]
    public float perBulletDelayMax = 0.15f;

    private float nextReadyTime;
    private Camera cam;
    private bool spawning; // tránh chồng coroutine (không bắt buộc vì đã có cooldown)

    void Awake()
    {
        cam = Camera.main;
        // Đọc trạng thái mở khóa đã lưu (vĩnh viễn qua scene & lần chạy)
        isUnlocked = PlayerPrefs.GetInt(PrefKey, 0) == 1;
    }

    void OnEnable()
    {
        // Phòng trường hợp Player được tái tạo sau khi Unlock đã lưu
        isUnlocked = PlayerPrefs.GetInt(PrefKey, 0) == 1;
    }

    void Update()
    {
        if (!isUnlocked) return;                 // chưa mở thì phím không có tác dụng
        if (Input.GetKeyDown(castKey)) TryCast();
    }

    public void Unlock()
    {
        isUnlocked = true;
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Rain of Bullets skill unlocked (persisted)!");
    }

    public float CooldownRemaining() => Mathf.Max(0f, nextReadyTime - Time.time);

    private void TryCast()
    {
        if (Time.time < nextReadyTime) return;
        if (!projectilePrefab) return;
        if (spawning) return;

        // đặt cooldown ngay khi kích hoạt
        nextReadyTime = Time.time + cooldownSeconds;
        StartCoroutine(SpawnRainRoutine());
    }

    private IEnumerator SpawnRainRoutine()
    {
        spawning = true;

        // đảm bảo tham số hợp lệ
        if (maxProjectiles < minProjectiles)
        {
            int t = maxProjectiles; maxProjectiles = minProjectiles; minProjectiles = t;
        }
        if (perBulletDelayMax < perBulletDelayMin)
        {
            float t = perBulletDelayMax; perBulletDelayMax = perBulletDelayMin; perBulletDelayMin = t;
        }

        int count = Random.Range(minProjectiles, maxProjectiles + 1);

        // Tính vùng spawn theo chế độ
        float xMin, xMax, ySpawn;
        if (coverWholeCameraWidth && cam && cam.orthographic)
        {
            float halfWidth = cam.orthographicSize * cam.aspect;
            xMin = cam.transform.position.x - halfWidth;
            xMax = cam.transform.position.x + halfWidth;
            ySpawn = cam.transform.position.y + cam.orthographicSize + spawnHeight;
        }
        else
        {
            Vector3 p = transform.position;
            xMin = p.x - horizontalRange;
            xMax = p.x + horizontalRange;
            ySpawn = p.y + spawnHeight;
        }

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(xMin, xMax);
            float y = ySpawn + Random.Range(-yJitter, yJitter); // lệch nhẹ theo Y
            Instantiate(projectilePrefab, new Vector3(x, y, 0f), Quaternion.identity);

            // delay ngẫu nhiên giữa các viên
            float wait = Random.Range(perBulletDelayMin, perBulletDelayMax);
            yield return new WaitForSeconds(wait);
        }

        spawning = false;
    }
}
