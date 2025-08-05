using UnityEngine;

public class PlaySingleVFXOnEnable : MonoBehaviour
{
    public ParticleSystem vfx;

    private void OnEnable()
    {
        if (vfx != null)
        {
            vfx.Play();
        }
    }
}