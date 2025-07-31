using GoogleMobileAds.Api;
using UnityEngine;

public class AdsInit : MonoBehaviour
{
    void Start()
    {
        MobileAds.Initialize(initStatus => { });
    }
}
