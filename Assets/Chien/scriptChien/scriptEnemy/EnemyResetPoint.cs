using UnityEngine;

public class EnemyResetPoint : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private IDamageable damageable;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        damageable = GetComponent<IDamageable>();
    }

    public void ResetEnemy()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        if (damageable != null)
        {
            //damageable.ResetHealth(); // Đảm bảo enemy implements interface này
        }

        gameObject.SetActive(true);
    }
}
