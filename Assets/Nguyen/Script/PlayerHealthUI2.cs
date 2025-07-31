using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI2 : MonoBehaviour
{
    public Slider healthSlider;

    private float displayHP;
    private float targetHP;
    private int maxHP;
    public float lerpSpeed = 15f;

    public void SetHealth(int current, int max)
    {
        maxHP = max;
        targetHP = current;

        // Gán ngay nếu chưa khởi tạo lần đầu
        if (displayHP == 0 || displayHP > maxHP)
            displayHP = targetHP;
    }

    private void Update()
    {
        if (maxHP <= 0) return;

        displayHP = Mathf.MoveTowards(displayHP, targetHP, lerpSpeed * Time.deltaTime);
        healthSlider.value = displayHP / maxHP;
    }
}
