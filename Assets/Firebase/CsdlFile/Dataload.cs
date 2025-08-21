using UnityEngine;
using UnityEngine.SceneManagement;
public class Dataload : MonoBehaviour
{
    private int loadDoneCount = 0;
    private const int totalToLoad = 5; // Cards + Gold

    // Hàm kiểm tra đã tải đủ chưa
    private void CheckAllLoaded()
    {
        loadDoneCount++;
        if (loadDoneCount >= totalToLoad)
        {
            // Sau khi tải cả Cards và Gold, chuyển scene
            SceneManager.LoadScene("SampleScene");
        }
    }

    // Gọi hàm này sau khi đăng nhập Firebase thành công
    public void LoadAllDataFromFirebase()
    {
        CardsManeger.LoadCardsFromFirebase(CheckAllLoaded);
        GoldManager.LoadGoldFromFirebase(CheckAllLoaded);
        lightningManeger.LoadLightningFromFirebase(CheckAllLoaded);
        CardSpingManeger.LoadCardSpingFromFirebase(CheckAllLoaded);
        TicketManeger.LoadTicketFromFirebase(CheckAllLoaded);
    }
}
