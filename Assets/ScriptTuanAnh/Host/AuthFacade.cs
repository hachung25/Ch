using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class AuthFacade : MonoBehaviour
{
    public static AuthFacade I;

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
                Debug.LogError("Firebase deps not available: " + dep);
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
            if (user != null)
            {
                string token = await user.TokenAsync(true);
                SecureTokenStore.Save(token, rememberMe, user.UserId);
            }
            else Debug.LogError("Register OK nhưng CurrentUser == null");
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
            if (user != null)
            {
                string token = await user.TokenAsync(true);
                SecureTokenStore.Save(token, rememberMe, user.UserId);
            }
            else Debug.LogError("Login OK nhưng CurrentUser == null");
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
