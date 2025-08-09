using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Text.RegularExpressions;
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

    [Header("Conflict Popup")]
    public GameObject conflictPopupPrefab;


    public TMP_Text logText;


    private FirebaseAuth auth;
    private Coroutine logCoroutine;

    private FireBaseDataBaseManager dataBaseManager;
    [SerializeField] private DeviceConflictManager conflictManager;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dataBaseManager = GetComponent<FireBaseDataBaseManager>();
        conflictManager = GetComponent<DeviceConflictManager>();

    }

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (conflictManager == null)
            conflictManager = GetComponent<DeviceConflictManager>();
        if (conflictManager == null)
            conflictManager = FindObjectOfType<DeviceConflictManager>(true);
        if (conflictManager == null)
            conflictManager = gameObject.AddComponent<DeviceConflictManager>();
    }

    public void SwitchToForgotPasswordForm()
    {
        LoginForm.SetActive(false);
        RegisterForm.SetActive(false);
        ForgotPasswordForm.SetActive(true);

        // Xóa input và log
        ipLoginEmail.text = "";
        ipLoginPassword.text = "";
        logText.text = "";
    }

    public void SwitchToLoginForm()
    {
        ForgotPasswordForm.SetActive(false);
        RegisterForm.SetActive(false);
        LoginForm.SetActive(true);

        // Xóa input và log
        ipResetEmail.text = "";
        logText.text = "";
    }


    private void LogToText(string message, System.Action onComplete = null)
    {
        if (logText == null) return;

        if (logCoroutine != null)
        {
            StopCoroutine(logCoroutine);
        }

        logText.text = message;
        logCoroutine = StartCoroutine(HideLogAfterDelay(2f, onComplete));
    }

    private IEnumerator HideLogAfterDelay(float delay, System.Action onComplete = null)
    {
        yield return new WaitForSeconds(delay);
        logText.text = "";
        onComplete?.Invoke();
    }

    private string IsValidGoogleEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "Email không được để trống!";
        if (email.Contains(" ")) return "Email không được chứa dấu cách!";

        string userName = email.Split('@')[0];
        if (userName.Length < 6) return "Tên tài khoản phải có ít nhất 6 ký tự!";
        if (userName.Length > 30) return "Tên tài khoản không được quá 30 ký tự!";
var emailPattern = @"^[a-zA-Z0-9_+&*-]+(?:\.[a-zA-Z0-9_+&*-]+)*@(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,7}$";
        if (!Regex.IsMatch(email, emailPattern)) return "Địa chỉ email không hợp lệ!";
        if (email.StartsWith(".") || email.EndsWith(".")) return "Email không được bắt đầu hoặc kết thúc bằng dấu chấm!";
        if (email.Contains("..")) return "Email không được chứa dấu chấm liên tiếp!";

        return null;
    }

    private string ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return "Mật khẩu không được để trống!";
        if (password.Contains(" ")) return "Mật khẩu không được chứa dấu cách!";
        if (password.Length < 8) return "Mật khẩu phải có ít nhất 8 ký tự!";

        bool hasLetter = false, hasDigit = false;
        foreach (char c in password)
        {
            if (char.IsLetter(c)) hasLetter = true;
            else if (char.IsDigit(c)) hasDigit = true;
        }

        if (!hasLetter) return "Mật khẩu phải có ít nhất một chữ cái!";
        if (!hasDigit) return "Mật khẩu phải có ít nhất một chữ số!";

        return null;
    }

    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;
        string confirmPassword = ipRegisterConfirmPassword.text;

        string emailError = IsValidGoogleEmail(email);
        if (!string.IsNullOrEmpty(emailError)) { LogToText(emailError); return; }

        string passwordError = ValidatePassword(password);
        if (!string.IsNullOrEmpty(passwordError)) { LogToText(passwordError); return; }

        if (string.IsNullOrEmpty(confirmPassword))
        {
            LogToText("Bạn chưa xác thực mật khẩu!");
            return;
        }

        if (password != confirmPassword)
        {
            LogToText("Xác thực mật khẩu không khớp!");
            return;
        }

        // Đăng ký trực tiếp
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(registerTask =>
        {
            if (registerTask.IsCanceled)
            {
                LogToText("Đăng ký bị hủy.");
            }
            else if (registerTask.IsFaulted)
            {
                FirebaseException firebaseEx = registerTask.Exception?.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                if (errorCode == AuthError.EmailAlreadyInUse)
                {
                    LogToText("Email đã được sử dụng!");
                }
                else
                {
                    LogToText("Đăng ký thất bạ "/* + firebaseEx.Message*/);
                }
            }
            else
            {
                LogToText("Tài khoản " + email + " đã được đăng ký thành công!", SwitchForm);
            }
        });
    }
public void SignInAccountWithFirebase()
    {
        string email = ipLoginEmail.text;
        string password = ipLoginPassword.text;

        string emailError = IsValidGoogleEmail(email);
        if (!string.IsNullOrEmpty(emailError)) { LogToText(emailError); return; }

        string passwordError = ValidatePassword(password);
        if (!string.IsNullOrEmpty(passwordError)) { LogToText(passwordError); return; }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                LogToText("Đăng nhập bị hủy.");
                return;
            }

            if (task.IsFaulted)
            {
                string errorMessage = ParseFirebaseLoginError(task.Exception);
                LogToText(errorMessage);
                return;
            }

            if (task.IsCompleted)
            {
                LogToText("Đăng nhập thành công");
    
                FirebaseUser firebaseUser = task.Result.User;
                string userId = firebaseUser.UserId;
                string deviceId = GetDeviceID();

                // Ghi user mới nếu cần
                User userinGame = new("Username", 0, 0,0,0, 0, 0, 0);
                dataBaseManager.WriteDataBase("Users/" + userId, userinGame.ToString());

                // Load trạng thái map từ Firebase
                // Load trạng thái map từ Firebase
                dataBaseManager.LoadMode(userId, (mode) =>
                {
                    Debug.Log("Trạng thái mode: " + mode); // false = map1, true = map2

                    // Gọi controller UI để xử lý giao diện map
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
                // Gọi các chức năng khác nếu cần
                if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                {
                    SaveManeger.LoadDailylogin();
                }

                FindObjectOfType<Dataload>().LoadAllDataFromFirebase();

                /// Tạo watcher
                GameObject watcherGO = new GameObject("OnlineStatusWatcher");
                DontDestroyOnLoad(watcherGO);
                var watcher = watcherGO.AddComponent<OnlineStatusWatcher>();
                watcher.conflictPopupPrefab = conflictPopupPrefab;
                watcher.StartWatching(userId, deviceId);

                // Ghi device info
                conflictManager.WriteFullDeviceInfo(userId, deviceId);

                // Đăng ký dọn dẹp khi disconnect
                Firebase.Database.FirebaseDatabase.DefaultInstance
                    .GetReference($"deviceStatus/{userId}/deviceId")
                    .OnDisconnect()
                    .SetValue(null);


                


            }

        });
    }

    public void loaddataBaseManager()
    {
        CardsManeger.LoadCardsFromFirebase();
        
    }

    private void OnApplicationQuit()
    {
        try
        {
            if (auth != null && auth.CurrentUser != null)
            {
                string userId = auth.CurrentUser.UserId;

                if (conflictManager != null)
                {
                    conflictManager.WriteDeviceStatus(userId, "deviceId", null);
                }
                else
                {
                    Firebase.Database.FirebaseDatabase.DefaultInstance
                        .GetReference($"deviceStatus/{userId}/deviceId")
                        .SetValueAsync(null);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"OnApplicationQuit cleanup failed: {e.Message}");
        }
    }




    public static string GetDeviceID()
    {
        string key = "LocalDeviceID_" + Application.identifier;
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetString(key, System.Guid.NewGuid().ToString());
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetString(key);
    }




    private string ParseFirebaseLoginError(System.AggregateException exception)
    {
        var baseException = exception?.GetBaseException();
        var firebaseEx = baseException as FirebaseException;

        if (firebaseEx == null) return "Lỗi đăng nhập không xác định.";

        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        switch (errorCode)
        {
            case AuthError.InvalidEmail:
case AuthError.WrongPassword:
                return "Sai tài khoản hoặc mật khẩu.";
            case AuthError.UserNotFound:
                return "Tài khoản của bạn chưa được đăng ký.";
            case AuthError.UserDisabled:
                return "Tài khoản đã bị vô hiệu hóa.";
            default:
                return "Lỗi đăng nhập" /*+ firebaseEx.Message*/;
        }
    }

    public void SwitchForm()
    {
        // Đảo trạng thái của hai form
        bool isLoginActive = !LoginForm.activeSelf;
        LoginForm.SetActive(isLoginActive);
        RegisterForm.SetActive(!isLoginActive);

        // Xóa dữ liệu khi chuyển form
        if (isLoginActive)
        {
            // Xóa dữ liệu form đăng ký
            ipRegisterEmail.text = "";
            ipRegisterPassword.text = "";
            ipRegisterConfirmPassword.text = "";
        }
        else
        {
            // Xóa dữ liệu form đăng nhập
            ipLoginEmail.text = "";
            ipLoginPassword.text = "";
        }

        // Xóa cả log nếu cần
        logText.text = "";
    }

    public void ResetPassword()
    {
        string email = ipResetEmail.text;

        string emailError = IsValidGoogleEmail(email);
        if (!string.IsNullOrEmpty(emailError))
        {
            LogToText(emailError);
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                LogToText("Yêu cầu đặt lại mật khẩu đã bị hủy.");
            }
            else if (task.IsFaulted)
            {
                FirebaseException firebaseEx = task.Exception?.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                switch (errorCode)
                {
                    case AuthError.InvalidEmail:
                        LogToText("Email không hợp lệ.");
                        break;
                    case AuthError.UserNotFound:
                        LogToText("Không tìm thấy tài khoản với email này.");
                        break;
                    default:
                        LogToText("Lỗi đặt lại mật khẩu "/* + firebaseEx.Message*/);
                        break;
                }
            }
            else
            {
                LogToText("Yêu cầu đặt lại mật khẩu đã được gửi! Vui lòng kiểm tra email.");
                
            }
        });
    }

    public void Logout()
    {
        // 1) Cleanup deviceId trên Firebase nếu còn đăng nhập
        if (auth != null && auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            if (conflictManager != null)
                conflictManager.WriteDeviceStatus(userId, "deviceId", null);
            else
                Firebase.Database.FirebaseDatabase.DefaultInstance
                    .GetReference($"deviceStatus/{userId}/deviceId")
                    .SetValueAsync(null);
        }

        // 2) Dừng watcher nếu đang chạy
        var watcher = FindObjectOfType<OnlineStatusWatcher>(true);
        if (watcher != null) watcher.StopWatching(); // sẽ tự Destroy

        // 3) Sign out và về Login
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginTA");
    }




}