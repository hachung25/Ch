//using UnityEngine;

//public class PlayerData : MonoBehaviour
//{
//    public Vector2 savedPosition;

//    public static PlayerData Instance;
//    public Vector2 defaultPosition = new Vector2(0, 0); // Set vị trí mặc định
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    public void ResetPlayer()
//    {
//        transform.position = defaultPosition;

//        Rigidbody2D rb = GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.linearVelocity = Vector2.zero;
//            rb.angularVelocity = 0f;
//        }

//        // Reset máu
//        var health = GetComponent<PlayerHealth>();
//        if (health != null) health.ResetHealth();

//        // Reset animation nếu cần
//        var anim = GetComponent<Animator>();
//        if (anim != null) anim.Rebind();
//    }
//}
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public Vector2 defaultPosition;
    public Vector2 savedPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        savedPosition = defaultPosition;
    }

    public void ResetPlayer()
    {
        // Đặt lại vị trí
        transform.position = savedPosition;

        // Dừng mọi chuyển động vật lý
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset máu
        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.ResetHealth();
        }

        // Reset animation
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }
}
