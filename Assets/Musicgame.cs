using UnityEngine;

public class Musicgame : MonoBehaviour
{
    void Start()
    {
        // Lấy hoặc thêm AudioSource
        AudioSource src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();

        // Gán clip nhạc trong Inspector
        src.playOnAwake = true;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
        src.volume = 1f;

        // Nếu chưa phát thì phát
        if (!src.isPlaying)
            src.Play();
    }

}
