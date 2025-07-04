using System;
using UnityEngine;

public class ShowWin : MonoBehaviour
{
    public Animator animator;          // Animator bạn muốn điều khiển
    public string boolName = "isWin";  // Tên biến bool trong Animator

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool(boolName, true);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetBool(boolName, true);
        }
    }
}