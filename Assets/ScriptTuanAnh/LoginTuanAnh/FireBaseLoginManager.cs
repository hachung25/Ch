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

    [Header("Remember Me")]
    public Toggle rememberMe;

    public TMP_Text logText;

    private FirebaseAuth auth;
    private Coroutine logCoroutine;
    private FireBaseDataBaseManager dataBaseManager;
    [SerializeField] private DeviceConflictManager conflictManager;

    private static bool hasAutoLoggedIn = false;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        if (conflictManager == null)
            conflictManager = GetComponent<DeviceConflictManager>();
        if (conflictManager == null)
            conflictManager = FindObjectOfType<DeviceConflictManager>(true);
        if (conflictManager == null)
            conflictManager = gameObject.AddComponent<DeviceConflictManager>();

        dataBaseManager = GetComponent<FireBaseDataBaseManager>();
    }

    private void Start()
    {
        if (rememberMe != null)
            rememberMe.isOn = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        var user = auth.CurrentUser;
        bool remember = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        if (!hasAutoLoggedIn && user != null && !user.IsAnonymous && remember)
        {
            hasAutoLoggedIn = true;
            ProceedToGame(user);   // Gọi chung 1 luồng xử lý
        }
        else
        {
            if (LoginForm != null) LoginForm.SetActive(true);
        }
    }

    // ====================== LOGIN SUCCESS ======================
    private void ProceedToGame(FirebaseUser firebaseUser)
    {
        OnLoginSuccess(firebaseUser);

        // Gọi Dataload để load dữ liệu trước khi vào scene
        var dataload = FindObjectOfType<Dataload>();
        if (dataload != null)
        {
            dataload.LoadAllDataFromFirebase();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Dataload trong scene login, load thẳng scene chính.");
            SceneManager.LoadScene("SampleScene");
        }
    }

    private void OnLoginSuccess(FirebaseUser firebaseUser)
    {
        string userId = firebaseUser.UserId;
        string deviceId = GetDeviceID();
        string sessionId = GetSessionID();
        string fullDeviceKey = deviceId + "_" + sessionId;

        // Trước khi ghi thiết bị mới → xoá toàn bộ node cũ
        FirebaseDatabase.DefaultInstance
            .GetReference($"deviceStatus/{userId}")
            .RemoveValueAsync()
            .ContinueWithOnMainThread(_ =>
            {
                // Ghi dữ liệu mẫu (tuỳ chỉnh theo game của bạn)
                User userinGame = new("Username", 0, 0, 0, 0, 0, 0, 0);
                dataBaseManager.WriteDataBase("Users/" + userId, userinGame.ToString());

                // Load mode
                dataBaseManager.LoadMode(userId, (mode) =>
                {
                    MapUIController mapUI = FindObjectOfType<MapUIController>();
                    if (mapUI != null) mapUI.ShowMode(mode);
                });

                SaveManeger.LoadDailylogin();

                // Tạo watcher để tránh xung đột thiết bị
                GameObject watcherGO = new GameObject("OnlineStatusWatcher");
                DontDestroyOnLoad(watcherGO);
                var watcher = watcherGO.AddComponent<OnlineStatusWatcher>();
                watcher.conflictPopupPrefab = conflictPopupPrefab;
                watcher.StartWatching(userId, fullDeviceKey);

                // Ghi lại device hiện tại (kèm sessionId)
                conflictManager.WriteFullDeviceInfo(userId, fullDeviceKey);

                // Khi disconnect thì xoá dấu thiết bị
                FirebaseDatabase.DefaultInstance
                    .GetReference($"deviceStatus/{userId}/{fullDeviceKey}")
                    .OnDisconnect().SetValue(null);
            });
    }

    // ====================== SIGN IN ======================
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
            if (task.IsCanceled) { LogToText("Đăng nhập bị hủy."); return; }
            if (task.IsFaulted) { LogToText(ParseFirebaseLoginError(task.Exception)); return; }

            // Thành công
            LogToText("Đăng nhập thành công");

            var firebaseUser = task.Result.User;

            // Remember Me (lưu flag cho lần sau)
            if (rememberMe != null && rememberMe.isOn)
                PlayerPrefs.SetInt("RememberMe", 1);
            else
                PlayerPrefs.SetInt("RememberMe", 0);
            PlayerPrefs.Save();

            // Đi chung luồng với auto-login
            ProceedToGame(firebaseUser);
        });
    }

    // ====================== REGISTER ======================
    public void RegisterAccountWithFirebase()
    {
        string email = ipRegisterEmail.text;
        string password = ipRegisterPassword.text;
        string confirmPassword = ipRegisterConfirmPassword.text;

        string emailError = IsValidGoogleEmail(email);
        if (!string.IsNullOrEmpty(emailError)) { LogToText(emailError); return; }

        string passwordError = ValidatePassword(password);
        if (!string.IsNullOrEmpty(passwordError)) { LogToText(passwordError); return; }

        if (password != confirmPassword) { LogToText("Xác thực mật khẩu không khớp!"); return; }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(registerTask =>
        {
            if (registerTask.IsCanceled || registerTask.IsFaulted)
            {
                FirebaseException firebaseEx = registerTask.Exception?.GetBaseException() as FirebaseException;
                var errorCode = firebaseEx != null ? (AuthError)firebaseEx.ErrorCode : 0;

                if (errorCode == AuthError.EmailAlreadyInUse)
                    LogToText("Email đã được sử dụng!");
                else
                    LogToText("Đăng ký thất bại");
            }
            else
            {
                FirebaseUser newUser = registerTask.Result.User;
                string userId = newUser.UserId;

                DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(userId);
                userRef.Child("email").SetValueAsync(email);
                userRef.Child("createdAt").SetValueAsync(System.DateTime.UtcNow.ToString());
                userRef.Child("username").SetValueAsync("Player_" + Random.Range(1000, 9999));

                LogToText("Tài khoản " + email + " đã đăng ký thành công!", SwitchForm);
            }
        });
    }

    // ====================== RESET PASSWORD ======================
    public void ResetPassword()
    {
        string email = ipResetEmail.text;
        string emailError = IsValidGoogleEmail(email);
        if (!string.IsNullOrEmpty(emailError)) { LogToText(emailError); return; }

        FirebaseDatabase.DefaultInstance
            .GetReference("Users")
            .OrderByChild("email").EqualTo(email)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    LogToText("Không thể kiểm tra email trong Database.");
                    return;
                }

                DataSnapshot snapshot = task.Result;
                if (snapshot == null || !snapshot.Exists)
                {
                    LogToText("Email này chưa được đăng ký!");
                    return;
                }

                auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(resetTask =>
                {
                    if (!resetTask.IsFaulted && !resetTask.IsCanceled)
                        LogToText("Yêu cầu đặt lại mật khẩu đã được gửi!");
                    else
                        LogToText("Lỗi đặt lại mật khẩu");
                });
            });
    }

    // ====================== LOGOUT ======================
    public void Logout()
    {
        if (auth != null && auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            // Xoá toàn bộ deviceStatus của user để tránh node thừa
            FirebaseDatabase.DefaultInstance
                .GetReference($"deviceStatus/{userId}")
                .RemoveValueAsync();
        }

        var watcher = FindObjectOfType<OnlineStatusWatcher>(true);
        if (watcher != null) watcher.StopWatching();

        auth.SignOut();

        // KHÔNG ép RememberMe = 0 ở đây
        SceneManager.LoadScene("LoginTA");
    }

    // ====================== HELPER ======================
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

    public static string GetSessionID()
    {
        return System.Guid.NewGuid().ToString(); // unique cho mỗi lần chạy app
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

    private void LogToText(string message, System.Action onComplete = null)
    {
        if (logText == null) return;
        if (logCoroutine != null) StopCoroutine(logCoroutine);
        logText.text = message;
        logCoroutine = StartCoroutine(HideLogAfterDelay(2f, onComplete));
    }

    private IEnumerator HideLogAfterDelay(float delay, System.Action onComplete = null)
    {
        yield return new WaitForSeconds(delay);
        logText.text = "";
        onComplete?.Invoke();
    }

    private string ParseFirebaseLoginError(System.AggregateException exception)
    {
        var firebaseEx = exception?.GetBaseException() as FirebaseException;
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
            case AuthError.NetworkRequestFailed:
                return "Lỗi mạng. Vui lòng kiểm tra kết nối.";
            default:
                return "Lỗi đăng nhập";
        }
    }

    public void SwitchForm()
    { // Đảo trạng thái của hai form
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

    // ====================== FORM FORGOT PASSWORD ======================
    public void MoveToForgotPassword()
    {
        if (LoginForm != null) LoginForm.SetActive(false);
        if (RegisterForm != null) RegisterForm.SetActive(false);
        if (ForgotPasswordForm != null) ForgotPasswordForm.SetActive(true);

        // Xóa dữ liệu cũ
        if (ipResetEmail != null) ipResetEmail.text = "";
        if (logText != null) logText.text = "";
    }

    public void BackToLoginFromForgot()
    {
        if (ForgotPasswordForm != null) ForgotPasswordForm.SetActive(false);
        if (LoginForm != null) LoginForm.SetActive(true);

        // Xóa dữ liệu cũ
        if (ipLoginEmail != null) ipLoginEmail.text = "";
        if (ipLoginPassword != null) ipLoginPassword.text = "";
        if (logText != null) logText.text = "";
    }
}
