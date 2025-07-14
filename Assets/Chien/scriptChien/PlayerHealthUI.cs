using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public PlayerHealth playerHealth; // Gắn từ editor hoặc tìm tự động

    [Header("Hiệu ứng")]
    public float lerpSpeed = 5f;

    private float displayedHealth;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>(); // fallback nếu chưa gán
        }

        displayedHealth = playerHealth.GetCurrentHealth();
        healthSlider.maxValue = playerHealth.maxHealth;
        healthSlider.value = displayedHealth;

        UpdateHealthText();
    }

    void Update()
    {
        displayedHealth = Mathf.Lerp(displayedHealth, playerHealth.GetCurrentHealth(), Time.deltaTime * lerpSpeed);
        healthSlider.value = displayedHealth;
        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        healthText.text = $"{playerHealth.GetCurrentHealth()} / {playerHealth.maxHealth}";
    }
}
