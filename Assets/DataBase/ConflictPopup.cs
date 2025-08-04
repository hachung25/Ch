using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConflictPopup : MonoBehaviour
{
    public TMP_Text messageText;
    public Button reloginButton;

    public Action onRelogin;

    private void Start()
    {
        messageText.text = "Tài khoản của bạn đang được đăng nhập ở nơi khác.";
        reloginButton.onClick.AddListener(() =>
        {
            onRelogin?.Invoke();
            Destroy(gameObject);
        });
    }
}
