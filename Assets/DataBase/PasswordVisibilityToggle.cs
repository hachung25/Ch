using UnityEngine;
using UnityEngine.UI;

public class PasswordVisibilityToggle_Classic : MonoBehaviour
{
    [Header("LOGIN")]
    public InputField loginPasswordInput;
    public GameObject showLoginPasswordButton;
    public GameObject hideLoginPasswordButton;

    [Header("REGISTER")]
    public InputField registerPasswordInput;
    public GameObject showRegisterPasswordButton;
    public GameObject hideRegisterPasswordButton;

    public InputField registerConfirmInput;
    public GameObject showRegisterConfirmButton;
    public GameObject hideRegisterConfirmButton;

    private bool isLoginVisible = false;
    private bool isRegisterVisible = false;
    private bool isConfirmVisible = false;

    public void ToggleLoginPassword()
    {
        isLoginVisible = !isLoginVisible;

        loginPasswordInput.contentType = isLoginVisible
            ? InputField.ContentType.Standard
            : InputField.ContentType.Password;

        loginPasswordInput.ForceLabelUpdate();

        showLoginPasswordButton.SetActive(!isLoginVisible);
        hideLoginPasswordButton.SetActive(isLoginVisible);
    }

    public void ToggleRegisterPassword()
    {
        isRegisterVisible = !isRegisterVisible;

        registerPasswordInput.contentType = isRegisterVisible
            ? InputField.ContentType.Standard
            : InputField.ContentType.Password;

        registerPasswordInput.ForceLabelUpdate();

        showRegisterPasswordButton.SetActive(!isRegisterVisible);
        hideRegisterPasswordButton.SetActive(isRegisterVisible);
    }

    public void ToggleRegisterConfirmPassword()
    {
        isConfirmVisible = !isConfirmVisible;

        registerConfirmInput.contentType = isConfirmVisible
            ? InputField.ContentType.Standard
            : InputField.ContentType.Password;

        registerConfirmInput.ForceLabelUpdate();

        showRegisterConfirmButton.SetActive(!isConfirmVisible);
        hideRegisterConfirmButton.SetActive(isConfirmVisible);
    }
}
