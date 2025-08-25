using UnityEngine;

public class showtrungthu : MonoBehaviour
{
   public GameObject Event2th9;
    public GameObject EventTrungthu;

    public void showEnenttrungthu()
    {
        Event2th9.SetActive(false);
        EventTrungthu.SetActive(true);
    }
}
