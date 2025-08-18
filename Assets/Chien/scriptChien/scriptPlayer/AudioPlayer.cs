using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip attackSound;
    private AudioSource audioSource;
    private bool hasAttackedOnce = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Gọi từ Animation Event
    // Gắn trên đúng frame trong animation Attack
    public void PlayAttackSound()
    {
        if (hasAttackedOnce) // nếu cần tránh phát lại lần đầu
            audioSource.PlayOneShot(attackSound);

        hasAttackedOnce = true;
    }

}
