using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void SceneLoad()
    {
          Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

}
