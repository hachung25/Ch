using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager I;
    private FirebaseAuth auth;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EarlyInit()
    {
        Application.wantsToQuit += () => { SecureTokenStore.DeleteIfNotRemembered(); return true; };
    }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep == DependencyStatus.Available)
            auth = FirebaseAuth.DefaultInstance;
        else
            Debug.LogError("Firebase deps not available: " + dep);
    }

    public async Task Register(string email, string password, bool rememberMe)
    {
        await auth.CreateUserWithEmailAndPasswordAsync(email, password);
        var user = auth.CurrentUser;
        if (user != null)
        {
            string token = await user.TokenAsync(true);
            SecureTokenStore.Save(token, rememberMe, user.UserId);
        }
    }

    public async Task Login(string email, string password, bool rememberMe)
    {
        await auth.SignInWithEmailAndPasswordAsync(email, password);
        var user = auth.CurrentUser;
        if (user != null)
        {
            string token = await user.TokenAsync(true);
            SecureTokenStore.Save(token, rememberMe, user.UserId);
        }
    }

    public void Logout()
    {
        auth?.SignOut();
        SecureTokenStore.TryDelete();
    }

    public string CurrentUserId => auth?.CurrentUser?.UserId;
    public string DisplayName => auth?.CurrentUser?.DisplayName ?? CurrentUserId;
}
