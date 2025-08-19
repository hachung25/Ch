using UnityEngine;

public class TeleportBox : MonoBehaviour
{
    private MapManager mapManager;
    public TbUnlockMap _Unlock;

    public AudioClip teleportSound;   // âm thanh bạn chọn
    private AudioSource audioSource;  // component phát âm
    private void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // nếu object chưa có AudioSource thì tự thêm
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Player collided with teleport box");
            // phát âm thanh
            if (teleportSound != null)
                audioSource.PlayOneShot(teleportSound);
            mapManager.MoveToNextMap();
        }
    }
    void OnEnable()
    {
        _Unlock.showTb();
    }

}