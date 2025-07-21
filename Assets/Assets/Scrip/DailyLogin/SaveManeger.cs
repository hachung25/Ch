using UnityEngine;
public static class SaveManeger 
{
   private const string Dailyloginkey = "DailyLoginSaveData";
   public static DailyLoginSaveData DailyloginData { get;private set; }

   static SaveManeger()
   {
      LoadDailylogin();
   }

   private static void LoadDailylogin()
   {
      string json = PlayerPrefs.GetString(Dailyloginkey,"");
      if (!string.IsNullOrEmpty(json))
      {
         DailyloginData = JsonUtility.FromJson<DailyLoginSaveData>(json);
      }
      else
      {
         DailyloginData = new DailyLoginSaveData();
      }
   }

   public static void SaveDailylogin()
   {
      string json = JsonUtility.ToJson(DailyloginData);
      PlayerPrefs.SetString(Dailyloginkey , json);
      PlayerPrefs.Save();
   }

   public static void ResetDailylogin()
   {
      PlayerPrefs.DeleteKey(Dailyloginkey);
      DailyloginData = new DailyLoginSaveData();
      SaveDailylogin();
   }
}
