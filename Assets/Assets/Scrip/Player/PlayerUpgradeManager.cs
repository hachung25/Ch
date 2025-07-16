using UnityEngine;

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

    private const string healthKey = "Upgrade_Health";
    private const string damageKey = "Upgrade_Damage";
    
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("GameInitialized"))
        {
            PlayerPrefs.SetInt(healthKey, baseHealth);
            PlayerPrefs.SetInt(damageKey, baseDamage);
            PlayerPrefs.SetInt("GameInitialized", 1);
            PlayerPrefs.Save();
            Debug.Log("Khởi tạo chỉ số ban đầu");
        }

        LoadStats();
    }

    public void UpgradeHealthSet(int amount)
    {
        amount = PlayerPrefs.GetInt("lightning");
        if (amount > 10)
        {
            lightningManeger.Spendlightning(10);
            currentHealth += healthUpgradeAmount;
            SaveHealth();
            Debug.Log($"Đã nâng cấp máu lên: {currentHealth}");
        }
        else
        {
            Debug.Log("Không đủ set để nâng cấp");
        }
        
     
    }  
    public void UpgradeHealthGold(int amount)
    {
        amount = PlayerPrefs.GetInt("Gold");
        if (amount > 100)
        {
                GoldManager.SpendGold(100);
                currentHealth += healthUpgradeAmount;
                SaveHealth();
                Debug.Log($"Đã nâng cấp máu lên: {currentHealth}");
        }
        else
        {
            Debug.Log("không đủ gold để nâng cấp");
        }
    
    }
    //
    public void UpgradeDamageSet(int amount)
    {
        amount = PlayerPrefs.GetInt("lightning");
        if (amount > 10)
        {
            lightningManeger.Spendlightning(10);
            currentDamage += damageUpgradeAmount;
            SaveDamage();
            Debug.Log($"Đã nâng cấp  lên: {currentDamage}");
            Debug.Log($"Đã nâng cấp Dame lên: {currentDamage}");
        }
        else
        {
            Debug.Log("Không đủ set để nâng cấp");
        }
        
     
    }  
    public void UpgradeDamageGold(int amount)
    {
        amount = PlayerPrefs.GetInt("Gold");
        if (amount > 100)
        {
            GoldManager.SpendGold(100);
            currentDamage += damageUpgradeAmount;
            SaveDamage();
            Debug.Log($"Đã nâng cấp Dame lên: {currentDamage}");
        }
        else
        {
            Debug.Log("không đủ gold để nâng cấp");
        }
    
    }
    
    private void LoadStats()
    {
        
        currentHealth = PlayerPrefs.GetInt(healthKey, baseHealth);
        currentDamage = PlayerPrefs.GetInt(damageKey, baseDamage);
    }

    public void SaveHealth()
    {
        PlayerPrefs.SetInt(healthKey, currentHealth);
        PlayerPrefs.Save();
    }

    public void SaveDamage()
    {
        PlayerPrefs.SetInt(damageKey, currentDamage);
        PlayerPrefs.Save();
    }

    public void ResetStats()
    {
        PlayerPrefs.DeleteKey(healthKey);
        PlayerPrefs.DeleteKey(damageKey);
        LoadStats();
    }
}