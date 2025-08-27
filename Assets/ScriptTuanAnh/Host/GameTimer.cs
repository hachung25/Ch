using Fusion;
using TMPro;
using UnityEngine;

public class GameTimer : NetworkBehaviour
{
    [Header("UI")]
    public TMP_Text timeText;          // Gắn Text_Time trong Inspector
    public GameObject rankingCanvas;   // Gắn Canvas Ranking trong Inspector

    [Header("Timer Settings")]
    public int startSeconds = 180;     // số giây ban đầu (vd: 180s = 3 phút)

    [Networked] private TickTimer Countdown { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority) // Host là người set thời gian
        {
            Countdown = TickTimer.CreateFromSeconds(Runner, startSeconds);
        }

        if (rankingCanvas) rankingCanvas.SetActive(false);
    }


    void Update()
    {
        if (Countdown.IsRunning)
        {
            int remain = Mathf.Max(0, (int)Countdown.RemainingTime(Runner));
            //Debug.Log("[GameTimer] remain = " + remain);

            if (timeText)
                timeText.text = remain.ToString("D2");

            if(remain == 0)
            {
                rankingCanvas.SetActive(true);
            }
        }
        
    }


}
