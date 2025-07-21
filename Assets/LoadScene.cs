using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void SceneLoad()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
