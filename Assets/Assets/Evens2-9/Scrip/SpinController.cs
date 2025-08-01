using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpinController : MonoBehaviour
{
    [Header("Thiết lập vòng quay")]
    public Transform wheel;              // Object Spin chứa các Slot
    public float spinDuration = 4f;
    public int segmentCount = 8;

    [Header("UI")]
    public Button spinButton;
    public TMP_Text rewardText;

    [Header("Slot phần thưởng")]
    public Transform[] rewardSlots;      // Gồm 8 slot theo thứ tự ngược chiều kim đồng hồ

    [Header("Mũi tên chỉ phần thưởng")]
    public Transform arrowTransform;     // Gắn mũi tên vào đây

    private bool isSpinning = false;

    public TB tb;

    void Start()
    {
        spinButton.onClick.AddListener(StartSpin);
    }

    public void StartSpin()
    {
        if (isSpinning) return;

        isSpinning = true;
        spinButton.interactable = false;

        float randomAngle = Random.Range(0f, 360f);
        float totalAngle = 360f * 5 + randomAngle;

        StartCoroutine(SpinWheel(totalAngle));
    }

    IEnumerator SpinWheel(float totalAngle)
    {
        float elapsed = 0f;
        float startAngle = wheel.eulerAngles.z;
        float endAngle = startAngle - totalAngle;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            float angle = Mathf.Lerp(startAngle, endAngle, EaseOutCubic(t));
            wheel.eulerAngles = new Vector3(0, 0, angle);
            yield return null;
        }

        wheel.eulerAngles = new Vector3(0, 0, endAngle);

        // 🎯 Xác định slot gần nhất với mũi tên theo khoảng cách
        int closestIndex = GetClosestSlotIndexByDistance();

        // 🎁 Lấy phần thưởng từ slot gần mũi tên
        Transform slot = rewardSlots[closestIndex];
        SpinRewardItem item = slot.GetComponentInChildren<SpinRewardItem>();

        if (item != null)
        {
            rewardText.text = $"Bạn nhận được: {item.rewardAmount} {item.rewardName}";
            

            // 👉 Thực hiện phần thưởng (debug hoặc thật)
            switch (item.rewardType)
            {
                case SpinRewardItem.RewardType.Coin:
                    tb.ShowTbSpins();
                    break;
                case SpinRewardItem.RewardType.Gem:
                    tb.ShowTbSpins();
                    break;
                case SpinRewardItem.RewardType.Cards:
                    tb.ShowTbSpins();
                    break;
                case SpinRewardItem.RewardType.CardSpin:
                    tb.ShowTbSpins();
                    break;
                case SpinRewardItem.RewardType.Ticket:
                    tb.ShowTbSpins();
                    break;
                default:
                    Debug.Log("🎁 Phần thưởng khác");
                    break;
            }
        }
        else
        {
            rewardText.text = "Không có phần thưởng!";
            Debug.LogWarning("⚠️ Không tìm thấy SpinRewardItem trong Slot được chọn.");
        }

        isSpinning = false;
        spinButton.interactable = true;
    }

    /// 📍 Hàm tìm slot gần mũi tên nhất bằng khoảng cách thực tế (không dùng góc)
    int GetClosestSlotIndexByDistance()
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < rewardSlots.Length; i++)
        {
            float distance = Vector3.Distance(rewardSlots[i].position, arrowTransform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }

    /// 🎚 Hàm easing
    float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}
