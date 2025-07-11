using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class FireBaseLoginManager : MonoBehaviour
{
    public InputField ipRegisterEmail;
    public InputField ipRegisterPassword;

    public Button buttonRegister;

    // FireBase Authentication --> Đăng kí, Đăng nhập
    private FirebaseAuth auth;

}
