//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class PortalTrigger : MonoBehaviour
//{
//    public string nextSceneName;
//    public Vector2 spawnPositionInNextScene;

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            if (EnemyManager.Instance != null && EnemyManager.Instance.AllEnemiesDead())
//            {
//                PlayerData.Instance.savedPosition = spawnPositionInNextScene;
//                SceneManager.LoadScene(nextSceneName);
//            }
//            else
//            {
//                Debug.Log("Chưa tiêu diệt hết quái! Không thể vào cổng.");
//            }
//        }
//    }
//}
