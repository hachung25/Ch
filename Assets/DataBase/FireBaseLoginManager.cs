using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class FireBaseLoginManager : MonoBehaviour
{
    // Đăng ký 
    [Header("Register")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;

    public Button buttonRegister;

    // Đăng nhập
    [Header("Sign in")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;

    public Button buttonLogin;

    // FireBase Authentication --> Đăng ký, Đăng nhập
    private FirebaseAuth auth;

    // Chuyển đổi qua lại giữa đăng ký và đăng nhập
    [Header("Switch Form")]
    public Button buttonMoveToSignIn;
    public Button buttonMoveToRegister;

    public GameObject LoginForm;
    public GameObject RegisterForm;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        buttonRegister.onClick.AddListener(RegisterAccountWithFirebase);
        buttonLogin.onClick.AddListener(SignInAccountWithFirebase);

        buttonMoveToRegister.onClick.AddListener(SwitchForm);
        buttonMoveToSignIn.onClick.AddListener(SwitchForm);
    }

    // Hàm đăng ký
    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text;

        string password = ipRegisterPassword.text;

        // Kiểm tra email và mật khẩu có trống không
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Email hoặc mật khẩu không được để trống!");
            return;
        }

        // Kiểm tra mật khẩu có đủ dài (ít nhất 6 ký tự)
        if (password.Length < 6)
        {
            Debug.Log("Mật khẩu phải có ít nhất 6 ký tự!");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {

            if (task.IsCanceled)
            {
                Debug.Log("Dang ky bi huy");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.Log("Dang ky that bai: " + task.Exception);
            }

            if (task.IsCompleted)
            {
                Debug.Log("Dang ky thanh cong");
            }
        });
    }

    // Hàm đăng nhập

    public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail.text;

        string password = ipLoginPassword.text;

        // Kiểm tra email và mật khẩu có trống không
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Email hoặc mật khẩu không được để trống!");
            return;
        }

        // Kiểm tra mật khẩu có đủ dài (ít nhất 6 ký tự)
        if (password.Length < 6)
        {
            Debug.Log("Mật khẩu phải có ít nhất 6 ký tự!");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("Dang nhap bi huy");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.Log("Dang nhap that bai: " + task.Exception);
            }

            if (task.IsCompleted)
            {
                // Thành công
                Debug.Log("Dang nhap thanh cong");

                // Chuyển màn chơi sau khi đăng nhập thành công
                SceneManager.LoadScene("SampleScene");
            }

        });
        
        
    }

    public void SwitchForm()
    {
        LoginForm.SetActive(!LoginForm.activeSelf);
        RegisterForm.SetActive(!RegisterForm.activeSelf);
    }


}
