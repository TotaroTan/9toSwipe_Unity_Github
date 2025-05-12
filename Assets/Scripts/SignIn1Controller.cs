using UnityEngine;
using UnityEngine.UI; // Required for Button

public class SignIn1Controller : MonoBehaviour
{
    public Button registerButton;
    public Button logInButton;

    void Start()
    {
        registerButton.onClick.AddListener(OnRegisterClicked);
        logInButton.onClick.AddListener(OnLogInClicked);
    }

    void OnRegisterClicked()
    {
        Debug.Log("Register button clicked");
        UIManager.Instance.ShowRegisterScreen();
    }

    void OnLogInClicked()
    {
        Debug.Log("Log In button clicked on SignIn1");
        UIManager.Instance.ShowSignIn2Screen();
    }
}