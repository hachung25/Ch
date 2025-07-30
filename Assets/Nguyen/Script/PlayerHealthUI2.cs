using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI2 : MonoBehaviour
{
    public Slider slider;

    public void SetHealth(int current, int max)
    {
        if (slider != null)
        {
            slider.value = (float)current / max;
        }
    }
}
