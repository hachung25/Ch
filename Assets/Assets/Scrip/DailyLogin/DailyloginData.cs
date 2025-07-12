using UnityEngine;
using TMPro;
public class DailyloginData : MonoBehaviour
{
    [System.Serializable]
   public class DaySlot
   {
      public TextMeshProUGUI dayText;
      public TextMeshProUGUI amountText;
   }
   
   public DailyloginScriptableojbect ManagerData;
   public DaySlot[] daySlot = new DaySlot[7];

   private void Start()
   {
       if (ManagerData != null && ManagerData.rewards != null)
       {
           for (int i = 0; i < daySlot.Length && i < ManagerData.rewards.Count; i++)
           {
               daySlot[i].dayText.text = "Days "+ManagerData.rewards[i].day;
               daySlot[i].amountText.text = ""+ManagerData.rewards[i].amount;
           }
       }
   }
}
