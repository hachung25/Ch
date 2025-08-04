using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FireBaseLoginManager : MonoBehaviour
{
    [Header("Register")]
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;
    public InputField ipRegisterConfirmPassword;
    public Button buttonRegister;

    [Header("Sign in")]
    public InputField ipLoginEmail;
    public InputField ipLoginPassword;
    public Button buttonLogin;

    [Header("Switch Form")]
    public Button buttonMoveToSignIn;
    public Button buttonMoveToRegister;
    public GameObject LoginForm;
    public GameObject RegisterForm;

    [Header("Forgot Password")]
    public InputField ipResetEmail;
    public Button buttonResetPassword;
    public GameObject ForgotPasswordForm;
    public Button buttonMoveToForgot;
    public Button buttonBackToLoginFromForgot;

    public TMP_Text logText;

    private FirebaseAuth auth;
    private Coroutine logCoroutine;
    private FireBaseDataBaseManager dataBaseManager;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dataBaseManager = GetComponent<FireBaseDataBaseManager>();
    }

    public void SwitchToForgotPasswordForm()
    {
        LoginForm.SetActive(false);
        RegisterForm.SetActive(false);
        ForgotPasswordForm.SetActive(true);
        ipLoginEmail.text = "";
        ipLoginPassword.text = "";
        logText.text = "";
    }

    public void SwitchToLoginForm()
    {
        ForgotPasswordForm.SetActive(false);
        RegisterForm.SetActive(false);
        LoginForm.SetActive(true);
        ipResetEmail.text = "";
        logText.text = "";
    }

    private void LogToText(string message, System.Action onComplete = null)
    {
        if (logText == null) return;

        if (logCoroutine != null)
            StopCoroutine(logCoroutine);

        logText.text = message;
        logCoroutine = StartCoroutine(HideLogAfterDelay(2f, onComplete));
    }

    private IEnumerator HideLogAfterDelay(float delay, System.Action onComplete = null)
    {
        yield return new WaitForSeconds(delay);
        logText.text = "";
        onComplete?.Invoke();
    }

    public static string GetDeviceID()
    {
        if (!PlayerPrefs.HasKey("LocalDeviceID"))
        {
            string generatedId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("LocalDeviceID", generatedId);
            PlayerPrefs.Save();
        }

        return PlayerPrefs.GetString("LocalDeviceID");
    }

    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;
        string confirmPassword = ipRegisterConfirmPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            LogToText("Vui lòng điền đầy đủ thông tin!");
            return;
        }

        if (password != confirmPassword)
        {
            LogToText("Xác thực mật khẩu không khớp!");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                LogToText("Đăng ký thất bại.");
                return;
            }

            LogToText("Đăng ký thành công!", SwitchForm);
        });
    }

    public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail.text;
        string password = ipLoginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LogToText("Vui lòng nhập email và mật khẩu.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                LogToText("Sai tài khoản hoặc mật khẩu.");
                return;
            }

            if (task.IsCompleted)
            {
                LogToText("Đăng nhập thành công");

                FirebaseUser firebaseUser = task.Result.User;
                string userId = firebaseUser.UserId;
                string deviceId = GetDeviceID();

                // Ghi user mới nếu cần
                User userinGame = new("Username", 0, 0, 0, 0, 0, 0, 0);
                dataBaseManager.WriteDataBase("Users/" + userId, userinGame.ToString());

                // Load trạng thái map từ Firebase
                dataBaseManager.LoadMode(userId, (mode) =>
                {
                    Debug.Log("Trạng thái mode: " + mode); // false = map1, true = map2

                    MapUIController mapUI = FindObjectOfType<MapUIController>();
                    if (mapUI != null)
                    {
                        mapUI.ShowMode(mode);
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy MapUIController trong scene.");
                    }
                });

                // Kiểm tra thiết bị đã đăng nhập
                dataBaseManager.ReadDataBase("Users/" + userId + "/onlineStatus/deviceId", (storedDeviceId) =>
                {
                    if (!string.IsNullOrEmpty(storedDeviceId) && storedDeviceId != deviceId)
                    {
                        LogToText("Tài khoản của bạn đang được đăng nhập ở thiết bị khác.");
                        auth.SignOut();
                        return;
                    }

                    dataBaseManager.WriteDataBase("Users/" + userId + "/onlineStatus/deviceId", deviceId);

                    LogToText("Đăng nhập thành công", () =>
                    {
                        SceneManager.LoadScene("SampleScene");
                    });

                    GameObject watcherGO = new GameObject("OnlineStatusWatcher");
                    DontDestroyOnLoad(watcherGO);
                    OnlineStatusWatcher watcher = watcherGO.AddComponent<OnlineStatusWatcher>();
                    watcher.StartWatching(userId, deviceId);
                });
            }
        });
    }


    private void OnApplicationQuit()
    {
        if (auth != null && auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            dataBaseManager.WriteDataBase("Users/" + userId + "/onlineStatus/deviceId", null);
        }
    }

    public void SwitchForm()
    {
        bool isLogin = LoginForm.activeSelf;
        LoginForm.SetActive(!isLogin);
        RegisterForm.SetActive(isLogin);

        ipLoginEmail.text = "";
        ipLoginPassword.text = "";
        ipRegisterEmail.text = "";
        ipRegisterPassword.text = "";
        ipRegisterConfirmPassword.text = "";
        logText.text = "";
        ForgotPasswordForm.SetActive(false);
    }

    public void ResetPassword()
    {
        string email = ipResetEmail.text;
        if (string.IsNullOrEmpty(email))
        {
            LogToText("Vui lòng nhập email.");
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
                LogToText("Không gửi được email đặt lại mật khẩu.");
            else
                LogToText("Đã gửi yêu cầu đặt lại mật khẩu. Vui lòng kiểm tra email.");
        });
    }
}
