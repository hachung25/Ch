using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public PlayerHealth playerHealth;

    [Header("Hiệu ứng")]
    public float lerpSpeed = 5f;

    private float displayedHealth;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        StartCoroutine(WaitForHealthReady());
    }

    private IEnumerator WaitForHealthReady()
    {
        float timeout = 5f;
        float timer = 0f;

        // Đợi đến khi PlayerHealth khởi tạo máu xong
        while (playerHealth.MaxHealth == 0 && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerHealth.MaxHealth == 0)
        {
            Debug.LogWarning("Không lấy được MaxHealth từ PlayerHealth. Gán mặc định 100.");
            playerHealth.ResetHealth(); // fallback nếu muốn reset
        }

        displayedHealth = playerHealth.GetCurrentHealth();
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = displayedHealth;

        UpdateHealthText();
    }

    void Update()
    {
        if (playerHealth == null || playerHealth.MaxHealth == 0)
            return;

        displayedHealth = Mathf.Lerp(displayedHealth, playerHealth.GetCurrentHealth(), Time.deltaTime * lerpSpeed);
        healthSlider.value = displayedHealth;

        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        if (healthText != null && playerHealth != null)
        {
            healthText.text = $"{playerHealth.GetCurrentHealth()} / {playerHealth.MaxHealth}";
        }
    }
}
