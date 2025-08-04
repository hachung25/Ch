using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSpawner : MonoBehaviour
{
    public void loadScenemap()
    {
        SceneManager.LoadScene("Mode1");
    } 
    public void loadScenemap2()
    {
        SceneManager.LoadScene("Mode2");
    }
    
}
