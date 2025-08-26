using UnityEngine;
using UnityEngine.UI;

public class VolumePlayManeger : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource audioSource;
    private const string VolumeKey = "Volume";

    void Start()
    {
       
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.value = savedVolume;
        audioSource.volume = savedVolume;

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        audioSource.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }
}
