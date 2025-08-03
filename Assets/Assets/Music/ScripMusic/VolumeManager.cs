using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource audioSource;
    private const string VolumeKey = "Volume";

    void Start()
    {
        // Lấy âm lượng đã lưu
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.value = savedVolume;
        audioSource.volume = savedVolume;

        // Gắn sự kiện khi thay đổi
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        audioSource.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }
}