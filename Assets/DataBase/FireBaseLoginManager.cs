using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using Firebase;
using System.Text.RegularExpressions;
using TMPro;

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

    // Tham chiếu đến TextMeshPro
    public TMP_Text logText;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        buttonRegister.onClick.AddListener(RegisterAccountWithFirebase);
        buttonLogin.onClick.AddListener(SignInAccountWithFirebase);

        buttonMoveToRegister.onClick.AddListener(SwitchForm);
        buttonMoveToSignIn.onClick.AddListener(SwitchForm);
    }

    // Hàm ghi log lên Text UI
    private void LogToText(string message)
    {
        if (logText != null)
        {
            logText.text += message + "\n";
        }
    }

    // Hàm đăng ký
    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;

        // Kiểm tra email và mật khẩu có trống không
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LogToText("Email hoặc mật khẩu không được để trống!");
            return;
        }

        // Kiểm tra mật khẩu theo quy tắc của Google
        string passwordError = ValidatePassword(password);
        if (passwordError != null)
        {
            LogToText(passwordError);
            return;
        }

        // Kiểm tra định dạng email hợp lệ theo quy tắc của Google
        string emailError = IsValidGoogleEmail(email);
        if (emailError != null)
        {
            LogToText(emailError);
            return;
        }

        // Kiểm tra email đã tồn tại chưa
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                LogToText("Email chưa tồn tại, tiến hành đăng ký.");

                auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(registerTask =>
                {
                    if (registerTask.IsCanceled)
                    {
                        LogToText("Đăng ký bị hủy");
                        return;
                    }

                    if (registerTask.IsFaulted)
                    {
                        LogToText("Đăng ký thất bại: " + registerTask.Exception);
                        return;
                    }

                    if (registerTask.IsCompleted)
                    {
                        LogToText("Đăng ký thành công");
                        LogToText("Tài khoản " + email + " đã được đăng ký thành công!");
                    }
                });
            }
            else
            {
                LogToText("Tài khoản với email " + email + " đã tồn tại!");
            }
        });
    }

    private string IsValidGoogleEmail(string email)
    {
        string userName = email.Split('@')[0];
        if (userName.Length < 6) return "Tên tài khoản phải có ít nhất 6 ký tự!";
        if (userName.Length > 30) return "Tên tài khoản không được quá 30 ký tự!";
        if (email.Contains(" ")) return "Email không được chứa dấu cách!";

        var emailPattern = @"^[a-zA-Z0-9_+&*-]+(?:\.[a-zA-Z0-9_+&*-]+)*@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,7}$";
        var regex = new Regex(emailPattern);
        if (!regex.IsMatch(email)) return "Địa chỉ email không hợp lệ!";
        if (email[0] == '.' || email[email.Length - 1] == '.') return "Email không được bắt đầu hoặc kết thúc bằng dấu chấm!";
        if (email.Contains("..")) return "Email không được chứa dấu chấm liên tiếp!";

        return null;
    }

    private string ValidatePassword(string password)
    {
        if (password.Length < 8) return "Mật khẩu phải có ít nhất 8 ký tự!";
        bool hasLetter = false, hasDigit = false, hasSpecialChar = false;

        foreach (char c in password)
        {
            if (char.IsLetter(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;
            if (!char.IsLetterOrDigit(c)) hasSpecialChar = true;
        }

        if (!hasLetter) return "Mật khẩu phải có ít nhất một chữ cái!";
        if (!hasDigit) return "Mật khẩu phải có ít nhất một chữ số!";
        if (!hasSpecialChar) return "Mật khẩu phải có ít nhất một ký tự đặc biệt!";

        return null;
    }

    public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail.text;
        string password = ipLoginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LogToText("Email hoặc mật khẩu không được để trống!");
            return;
        }

        string emailError = IsValidGoogleEmail(email);
        if (emailError != null)
        {
            LogToText(emailError);
            return;
        }

        string passwordError = ValidatePassword(password);
        if (passwordError != null)
        {
            LogToText(passwordError);
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                LogToText("Đăng nhập bị hủy");
                return;
            }

            if (task.IsFaulted)
            {
                LogToText("Lỗi đăng nhập: " + task.Exception?.ToString());
                FirebaseException firebaseException = (FirebaseException)task.Exception?.InnerExceptions[0];
                AuthError errorCode = (AuthError)firebaseException.ErrorCode;

                if (errorCode == AuthError.InvalidEmail || errorCode == AuthError.WrongPassword)
                {
                    LogToText("Sai tài khoản hoặc mật khẩu.");
                }
                else if (errorCode == AuthError.UserNotFound)
                {
                    LogToText("Tài khoản của bạn chưa được đăng ký.");
                }
                else
                {
                    LogToText("Lỗi đăng nhập: " + task.Exception?.Message);
                }

                return;
            }

            if (task.IsCompleted)
            {
                LogToText("Đăng nhập thành công");
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
