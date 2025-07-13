using UnityEngine;
using UnityEngine.UI;
using System;

public class DailyloginUI : MonoBehaviour
{
   public Button[] dayButtons;
   public GameObject[] dayPanels;
   public DailyloginManeger dailyloginManeger;
   public GameObject[] clamied;
   void Start()
   {
       SaveManeger.SaveDailylogin();
       UpdateLoginUI();
   }
   void UpdateLoginUI()
   {
      var data = SaveManeger.DailyloginData;
      int LoginDays = data.LoginDays;
      String lastLogin = data.DailyloginDate;
      string today = DateTime.Now.ToString("yyyyMMdd");
       bool canClaimtoday = lastLogin != today;
       if (LoginDays >= 7)
       {
           LoginDays = 0;
           SaveManeger.DailyloginData.LoginDays = 0;
           SaveManeger.SaveDailylogin();
       }
       for (int i = 0; i < 7; i++)
       {
           int index = i;
           dayButtons[i].interactable = false;
           dayPanels[i].SetActive(false);
           clamied[i].SetActive(false);

           if (i < LoginDays)
           {
               dayPanels[i].SetActive(true);
               clamied[i].SetActive(false);
           }
           else if (i == LoginDays && canClaimtoday)
           {
               clamied[i].SetActive(true);
               dayButtons[i].interactable = true;
               dayButtons[i].onClick.RemoveAllListeners();
               dayButtons[i].onClick.AddListener(()=>ClaiReward(index));
           }
       }
   }
   void ClaiReward(int index)
   {
       string today = DateTime.Now.ToString("yyyyMMdd");
       SaveManeger.DailyloginData.DailyloginDate = today;
       SaveManeger.DailyloginData.LoginDays = index + 1;
       SaveManeger.SaveDailylogin();

       if (dailyloginManeger != null)
       {
           dailyloginManeger.OnclamDailyReward(index);
       }

       if (index >= 0 && index < dayPanels.Length)
       {
           dayPanels[index].SetActive(true);
       }
      UpdateLoginUI();
   }
   public void SimulateNextDay()
   {
       SaveManeger.DailyloginData.DailyloginDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
       SaveManeger.SaveDailylogin();

       Debug.Log("Đã giả lập sang ngày tiếp theo. Bây giờ bạn có thể điểm danh.");
       UpdateLoginUI();
   }
}
