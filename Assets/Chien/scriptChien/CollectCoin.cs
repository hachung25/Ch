using Photon.Realtime;
using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    private bool isMagnetActive = false;
    private Transform player;
    public float flySpeed = 5f;

    void Update()
    {
        if (isMagnetActive)
        {
            if (player == null)
            {
                // Tự tìm lại Player mỗi frame nếu chưa có
                if (PlayerData.Instance != null)
                    player = PlayerData.Instance.transform;
                else
                    return; // chưa có player thì bỏ qua
            }

            transform.position = Vector2.MoveTowards(transform.position, player.position, flySpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.position) < 0.3f)
            {
                //GoldManager.Instance.AddGold(1);
                Destroy(gameObject);
            }
        }
    }

    public void ActivateMagnet()
    {
        isMagnetActive = true;
        GetComponent<Rigidbody2D>().simulated = false;
    }
}
