using UnityEngine;

public class UIDailyloginOnOf : MonoBehaviour
{
      public GameObject Onclick;
      public GameObject PanelDailyLogin;

      public void OnDailylogin()
      {
            Onclick.SetActive(false);
            PanelDailyLogin.SetActive(true);
      }  
      public void OnClickk()
      {
            Onclick.SetActive(true);
            PanelDailyLogin.SetActive(false);
      }
      
}
