using UnityEngine;
using System.Collections;
public class BreakableBox : MonoBehaviour, IDamageable
{
    [Header("Config")]
    public GameObject healthPickupPrefab; // Prefab cục máu
    public Transform spawnPoint;          // Vị trí spawn (nếu null thì dùng vị trí hộp)
    public int dropAmount = 1;            // Số cục máu rơi ra
    public bool isInvincible = false;
    [Header("Feedback")]
    public GameObject breakVFX;
    public AudioClip breakSfx;
    public GameObject VFXHealth;
    private bool isBroken = false;

    // Gọi khi Player tấn công (qua interface IDamageable)
    public void TakeDamage(int damage)
    {
        if (isBroken) return;
        BreakBox();
    }

    private void BreakBox()
    {
        isBroken = true;

        // Spawn máu
        if (healthPickupPrefab != null)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

                // Spawn máu
                var hpObj = Instantiate(healthPickupPrefab, pos, Quaternion.identity);

                // Spawn VFX và GÁN LÀM CON của máu
                if (VFXHealth != null)
                {
                    var vfx = Instantiate(VFXHealth, pos, Quaternion.identity, hpObj.transform);
                    vfx.transform.localPosition = Vector3.zero; // canh tâm (tùy)
                }
            }

        }

        // VFX
        if (breakVFX != null)
            Instantiate(breakVFX, transform.position, Quaternion.identity);

        // SFX
        if (breakSfx != null)
            AudioSource.PlayClipAtPoint(breakSfx, transform.position);
        StartCoroutine(FlashWhileInvincible());
        Destroy(gameObject, 0.5f); // Xóa cái hộp
    }

    private IEnumerator FlashWhileInvincible()
    {
        float duration = 0.5f;
        float timer = 0f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        while (timer < duration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        sr.enabled = true;
        isInvincible = false;
    }
}
