using System.Collections;
using UnityEngine;

public class RainOfBulletsSkill : MonoBehaviour
{
    // 🔑 Key PlayerPrefs duy nhất cho skill
    public const string PrefKey = "RainSkillUnlocked";

    [Header("Unlock State")]
    public bool isUnlocked = false;

    [Header("Reset / New Game")]
    public bool lockOnGameStart = false;

    [Header("Key & Cooldown")]
    public KeyCode castKey = KeyCode.K;
    public float cooldownSeconds = 10f;

    [Header("Projectile")]
    public BulletRainProjectile projectilePrefab;

    [Min(1)] public int minProjectiles = 1;
    [Min(1)] public int maxProjectiles = 20;

    [Header("Spawn Area / Pattern")]
    public bool coverWholeCameraWidth = false;
    public float spawnHeight = 10f;
    public float horizontalRange = 10f;
    public float yJitter = 1.5f;

    [Header("Timing")]
    public float perBulletDelayMin = 0.03f;
    public float perBulletDelayMax = 0.15f;

    private float nextReadyTime;
    private Camera cam;
    private bool spawning;

    void Awake()
    {
        cam = Camera.main;

        if (lockOnGameStart)
            Lock();

        LoadState();
    }

    void OnEnable()
    {
        LoadState();
    }

    private void LoadState()
    {
        isUnlocked = PlayerPrefs.GetInt(PrefKey, 0) == 1;
    }

    void Update()
    {
        if (!isUnlocked) return;
        if (Input.GetKeyDown(castKey)) TryCast();
    }

    // ==== API ====
    public void Unlock()
    {
        isUnlocked = true;
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();
        Debug.Log("RainSkill unlocked!");
    }

    public void Lock()
    {
        isUnlocked = false;
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
        Debug.Log("RainSkill locked!");
    }

    public static void ResetPersisted()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
        Debug.Log("RainSkill reset by New Game.");

        var skill = FindObjectOfType<RainOfBulletsSkill>();
        if (skill != null) skill.isUnlocked = false;
    }

    public static void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs cleared!");
    }

    public float CooldownRemaining() => Mathf.Max(0f, nextReadyTime - Time.time);

    private void TryCast()
    {
        if (Time.time < nextReadyTime) return;
        if (!projectilePrefab) return;
        if (spawning) return;

        nextReadyTime = Time.time + cooldownSeconds;
        StartCoroutine(SpawnRainRoutine());
    }

    private IEnumerator SpawnRainRoutine()
    {
        spawning = true;

        if (maxProjectiles < minProjectiles)
            (minProjectiles, maxProjectiles) = (maxProjectiles, minProjectiles);

        if (perBulletDelayMax < perBulletDelayMin)
            (perBulletDelayMin, perBulletDelayMax) = (perBulletDelayMax, perBulletDelayMin);

        int count = Random.Range(minProjectiles, maxProjectiles + 1);
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
            float y = ySpawn + Random.Range(-yJitter, yJitter);
            Instantiate(projectilePrefab, new Vector3(x, y, 0f), Quaternion.identity);

            float wait = Random.Range(perBulletDelayMin, perBulletDelayMax);
            yield return new WaitForSeconds(wait);
        }

        spawning = false;
    }
}