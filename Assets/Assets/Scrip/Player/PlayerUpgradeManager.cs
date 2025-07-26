using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("Chỉ số cơ bản")]
    public int baseHealth = 100;
    public int baseDamage = 10;

    [Header("Giá trị hiện tại")]
    public int currentHealth;
    public int currentDamage;

    [Header("Thông số nâng cấp")]
    public int healthUpgradeAmount = 20;
    public int damageUpgradeAmount = 5;

    private const string healthKey = "Health";
    private const string damageKey = "Damage";

    private DatabaseReference reference;
    private string userId;

    private void Awake()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        LoadStats();
    }

    public void UpgradeHealthSet(int dummy)
    {
        int amount = lightningManeger.GetLightning();
        if (amount >= 10)
        {
            lightningManeger.SpendLightning(10);
            currentHealth += healthUpgradeAmount;
            SaveHealth();
            Debug.Log($"Đã nâng cấp máu lên: {currentHealth}");
        }
        else
        {
            Debug.Log("Không đủ lightning để nâng cấp");
        }
    }

    public void UpgradeHealthGold(int dummy)
    {
        int amount = GoldManager.GetGold();
        if (amount >= 100)
        {
            GoldManager.SpendGold(100);
            currentHealth += healthUpgradeAmount;
            SaveHealth();
            Debug.Log($"Đã nâng cấp máu lên: {currentHealth}");
        }
        else
        {
            Debug.Log("Không đủ gold để nâng cấp");
        }
    }

    public void UpgradeDamageSet(int dummy)
    {
        int amount = lightningManeger.GetLightning();
        if (amount >= 10)
        {
            lightningManeger.SpendLightning(10);
            currentDamage += damageUpgradeAmount;
            SaveDamage();
            Debug.Log($"Đã nâng cấp dame lên: {currentDamage}");
        }
        else
        {
            Debug.Log("Không đủ lightning để nâng cấp");
        }
    }

    public void UpgradeDamageGold(int dummy)
    {
        int amount = GoldManager.GetGold();
        if (amount >= 100)
        {
            GoldManager.SpendGold(100);
            currentDamage += damageUpgradeAmount;
            SaveDamage();
            Debug.Log($"Đã nâng cấp dame lên: {currentDamage}");
        }
        else
        {
            Debug.Log("Không đủ gold để nâng cấp");
        }
    }

    private void LoadStats()
    {
        if (string.IsNullOrEmpty(userId)) return;

        reference.Child("Users").Child(userId).Child("Upgrade").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.HasChild(healthKey))
                    currentHealth = int.Parse(snapshot.Child(healthKey).Value.ToString());
                else
                    currentHealth = baseHealth;

                if (snapshot.HasChild(damageKey))
                    currentDamage = int.Parse(snapshot.Child(damageKey).Value.ToString());
                else
                    currentDamage = baseDamage;
            }
            else
            {
                currentHealth = baseHealth;
                currentDamage = baseDamage;
                Debug.LogWarning("Không thể tải chỉ số từ Firebase.");
            }
        });
    }

    public void SaveHealth()
    {
        if (string.IsNullOrEmpty(userId)) return;

        reference.Child("Users").Child(userId).Child("Upgrade").Child(healthKey).SetValueAsync(currentHealth);
    }

    public void SaveDamage()
    {
        if (string.IsNullOrEmpty(userId)) return;

        reference.Child("Users").Child(userId).Child("Upgrade").Child(damageKey).SetValueAsync(currentDamage);
    }

    public void ResetStats()
    {
        if (string.IsNullOrEmpty(userId)) return;

        reference.Child("Users").Child(userId).Child("Upgrade").Child(healthKey).RemoveValueAsync();
        reference.Child("Users").Child(userId).Child("Upgrade").Child(damageKey).RemoveValueAsync();

        currentHealth = baseHealth;
        currentDamage = baseDamage;
    }
}
