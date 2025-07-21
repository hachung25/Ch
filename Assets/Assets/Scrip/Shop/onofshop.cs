using UnityEngine;

public class onofshop : MonoBehaviour
{
   public GameObject panelShopGold;
 //  public GameObject panelShopKc;

   public void ShowShopGold()
   {
      panelShopGold.SetActive(true);
     // panelShopKc.SetActive(false);
   }
   
   public void ShowShopKC()
   {
     // panelShopKc.SetActive(true);
      panelShopGold.SetActive(false);
   }
}
