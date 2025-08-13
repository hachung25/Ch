using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class AuthFacade : MonoBehaviour
{
    public static AuthFacade I;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EarlyQuitHook() => Application.wantsToQuit += () => { SecureTokenStore.DeleteIfNotRemembered(); return true; };

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
            Debug.LogError("Firebase deps not available: " + dep);
    }

    public async Task Register(string email, string password, bool rememberMe)
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

    public async Task Login(string email, string password, bool rememberMe)
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

    public void Logout()
    {
        FirebaseAuth.DefaultInstance?.SignOut();
        SecureTokenStore.TryDelete();
    }

    public string CurrentUserId => FirebaseAuth.DefaultInstance?.CurrentUser?.UserId;
    public string DisplayName => FirebaseAuth.DefaultInstance?.CurrentUser?.DisplayName ?? CurrentUserId;
}
