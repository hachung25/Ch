using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class AuthFacade : MonoBehaviour
{
    public static AuthFacade I;
    private bool triedAutoLogin = false;   // ✅ Flag chống lặp

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EarlyQuitHook()
        => Application.wantsToQuit += () => { SecureTokenStore.DeleteIfNotRemembered(); return true; };

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        try
        {
            var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dep != DependencyStatus.Available)
            {
                Debug.LogError("Firebase deps not available: " + dep);
                return;
            }

            // Nếu đã có user -> không auto login nữa
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                Debug.Log("Đã có user active: " + FirebaseAuth.DefaultInstance.CurrentUser.UserId);
                return;
            }

            // Nếu chưa có user -> mới thử auto login từ SecureTokenStore
            var t = SecureTokenStore.TryLoad();
            if (t.ok && t.rememberMe)
            {
                Debug.Log("Thử auto login bằng token cache...");

                try
                {
                    // 👉 Firebase không cho sign-in trực tiếp bằng idToken string
                    // Nếu muốn auto login bằng email/password -> phải lưu email/password (không an toàn lắm)
                    // Khuyến nghị: dựa vào CurrentUser của Firebase, không cần token riêng
                    Debug.Log("Auto login bỏ qua vì Firebase tự duy trì session.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Auto login fail: " + e.Message);
                    SecureTokenStore.TryDelete();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Firebase init failed: " + e);
        }
    }


    public async Task Register(string email, string password, bool rememberMe)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null && rememberMe)
                SecureTokenStore.Save(email, password, rememberMe);
            else
                SecureTokenStore.TryDelete();
        }
        catch (Exception e)
        {
            Debug.LogError("Register error: " + e.Message);
            throw;
        }
    }

    public async Task Login(string email, string password, bool rememberMe)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null && rememberMe)
                SecureTokenStore.Save(email, password, rememberMe);
            else
                SecureTokenStore.TryDelete();
        }
        catch (Exception e)
        {
            Debug.LogError("Login error: " + e.Message);
            throw;
        }
    }

    public void Logout()
    {
        try
        {
            FirebaseAuth.DefaultInstance?.SignOut();
        }
        finally
        {
            SecureTokenStore.TryDelete();
        }
    }

    public string CurrentUserId => FirebaseAuth.DefaultInstance?.CurrentUser?.UserId;
    public string DisplayName => FirebaseAuth.DefaultInstance?.CurrentUser?.DisplayName ?? CurrentUserId;
}
