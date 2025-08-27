using UnityEngine;

public class DeleteBullet : MonoBehaviour
{
    //public void ClearAllData()
    //{
    //    PlayerPrefs.DeleteKey(RainOfBulletsSkill.PrefKey);

    //    PlayerPrefs.Save();
    //    Debug.Log("Rain of Bullets skill data cleared!");
    //}
    public void ResetRainSkill()
    {
        RainOfBulletsSkill.ResetPersisted();
    }
}
