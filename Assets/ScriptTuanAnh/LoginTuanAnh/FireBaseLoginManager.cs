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
    public Toggle rememberMe; // Toggle ở form Đăng nhập

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
        // 🔹 Set lại trạng thái toggle RememberMe từ PlayerPrefs
        if (rememberMe != null)
            rememberMe.isOn = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        var user = auth.CurrentUser;
        bool remember = PlayerPrefs.GetInt("RememberMe", 0) == 1;

        if (!hasAutoLoggedIn && user != null && !user.IsAnonymous && remember)
        {
            hasAutoLoggedIn = true;
            Debug.Log("Auto login thành công");
            OnLoginSuccess(user);
            SceneManager.LoadScene("SampleScene"); // đổi tên scene chính của bạn
        }
        else
        {
            Debug.Log("Chưa login hoặc chưa chọn RememberMe → ở lại màn hình login");
            if (LoginForm != null) LoginForm.SetActive(true);
        }
    }

    // ====================== XỬ LÝ CHUNG SAU KHI LOGIN ======================
    private void OnLoginSuccess(FirebaseUser firebaseUser)
    {
        string userId = firebaseUser.UserId;
        string deviceId = GetDeviceID();

        // Ghi dữ liệu mẫu
        User userinGame = new("Username", 0, 0, 0, 0, 0, 0, 0);
        dataBaseManager.WriteDataBase("Users/" + userId, userinGame.ToString());

        // Load mode
        dataBaseManager.LoadMode(userId, (mode) =>
        {
            Debug.Log("Trạng thái mode: " + mode);
            MapUIController mapUI = FindObjectOfType<MapUIController>();
            if (mapUI != null) mapUI.ShowMode(mode);
        });

        // Load daily login
        SaveManeger.LoadDailylogin();

        // Load toàn bộ data
        FindObjectOfType<Dataload>().LoadAllDataFromFirebase();

        // Watcher online
        GameObject watcherGO = new GameObject("OnlineStatusWatcher");
        DontDestroyOnLoad(watcherGO);
        var watcher = watcherGO.AddComponent<OnlineStatusWatcher>();
        watcher.conflictPopupPrefab = conflictPopupPrefab;
        watcher.StartWatching(userId, deviceId);

        conflictManager.WriteFullDeviceInfo(userId, deviceId);

        FirebaseDatabase.DefaultInstance
            .GetReference($"deviceStatus/{userId}/deviceId")
            .OnDisconnect().SetValue(null);
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

        if (string.IsNullOrEmpty(confirmPassword)) { LogToText("Bạn chưa xác thực mật khẩu!"); return; }
        if (password != confirmPassword) { LogToText("Xác thực mật khẩu không khớp!"); return; }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(registerTask =>
        {
            if (registerTask.IsCanceled)
            {
                LogToText("Đăng ký bị hủy.");
            }
            else if (registerTask.IsFaulted)
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
                // ✅ Tạo user thành công
                FirebaseUser newUser = registerTask.Result.User;
                string userId = newUser.UserId;

                // ✅ Lưu thông tin cơ bản vào Realtime Database
                DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(userId);
                userRef.Child("email").SetValueAsync(email);
                userRef.Child("createdAt").SetValueAsync(System.DateTime.UtcNow.ToString());

                // Nếu muốn có thêm username mặc định
                userRef.Child("username").SetValueAsync("Player_" + Random.Range(1000, 9999));

                LogToText("Tài khoản " + email + " đã được đăng ký thành công!", SwitchForm);
            }
        });
    }


    // ====================== LOGIN ======================
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

            // 🔹 Lưu trạng thái RememberMe
            if (rememberMe != null && rememberMe.isOn)
                PlayerPrefs.SetInt("RememberMe", 1);
            else
                PlayerPrefs.SetInt("RememberMe", 0);
            PlayerPrefs.Save();

            var firebaseUser = task.Result.User;
            OnLoginSuccess(firebaseUser);

            SceneManager.LoadScene("SampleScene"); // đổi tên scene chính
        });
    }

    // ====================== RESET PASSWORD ======================
    public void ResetPassword()
    {
        string email = ipResetEmail.text;
        string emailError = IsValidGoogleEmail(email);
        if (!string.IsNullOrEmpty(emailError)) { LogToText(emailError); return; }

        // Kiểm tra email trong Database trước
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
                    // Không tìm thấy email trong DB
                    LogToText("Email này chưa được đăng ký trong hệ thống!");
                    return;
                }

                // ✅ Email có tồn tại → gửi yêu cầu reset password
                auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(resetTask =>
                {
                    if (resetTask.IsCanceled)
                    {
                        LogToText("Yêu cầu đặt lại mật khẩu đã bị hủy.");
                    }
                    else if (resetTask.IsFaulted)
                    {
                        FirebaseException firebaseEx = resetTask.Exception?.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        if (errorCode == AuthError.InvalidEmail) LogToText("Email không hợp lệ.");
                        else if (errorCode == AuthError.UserNotFound) LogToText("Không tìm thấy tài khoản với email này.");
                        else LogToText("Lỗi đặt lại mật khẩu");
                    }
                    else
                    {
                        LogToText("Yêu cầu đặt lại mật khẩu đã được gửi! Vui lòng kiểm tra email.");
                    }
                });
            });
    }


    // ====================== LOGOUT ======================
    public void Logout()
    {
        if (auth != null && auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            if (conflictManager != null)
                conflictManager.WriteDeviceStatus(userId, "deviceId", null);
            else
                FirebaseDatabase.DefaultInstance
                    .GetReference($"deviceStatus/{userId}/deviceId")
                    .SetValueAsync(null);
        }

        var watcher = FindObjectOfType<OnlineStatusWatcher>(true);
        if (watcher != null) watcher.StopWatching();

        auth.SignOut();

        // 🔹 Xóa RememberMe khi logout
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginTA");
    }

    // ====================== HELPER ======================
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
            default:
                return "Lỗi đăng nhập";
        }
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


    public void SwitchForm()
    {
        // Đảo trạng thái của hai form
        bool isLoginActive = !LoginForm.activeSelf;
        LoginForm.SetActive(isLoginActive);
        RegisterForm.SetActive(!isLoginActive);

        // Xóa dữ liệu khi chuyển form
        if (isLoginActive)
        {
            ipRegisterEmail.text = "";
            ipRegisterPassword.text = "";
            ipRegisterConfirmPassword.text = "";
        }
        else
        {
            ipLoginEmail.text = "";
            ipLoginPassword.text = "";
        }

        // Xóa log nếu cần
        if (logText != null)
            logText.text = "";
    }
}
