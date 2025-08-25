using UnityEngine;

[System.Serializable]
public class SpinRewardItem : MonoBehaviour
{
    [Header("Thông tin phần thưởng")]
    public string rewardName;              // Ví dụ: "100 Coin", "50 Gem"
    public int rewardAmount = 0;           // Giá trị phần thưởng
    public RewardType rewardType = RewardType.Coin; // Loại phần thưởng

    public enum RewardType
    {
        Coin,
        Gem,
        Cards,
        CardSpin,
        Ticket
    }
}