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
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.T)) // khi bấm phím T
    //    {
    //        audioSource.PlayOneShot(attackSound); // phát âm thanh attack
    //    }
    //}
    public void PlayAttackSound()
    {
        if (hasAttackedOnce) // nếu cần tránh phát lại lần đầu
            audioSource.PlayOneShot(attackSound);

        hasAttackedOnce = true;
    }

}
