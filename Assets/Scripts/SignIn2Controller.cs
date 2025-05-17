using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class SignIn2Controller : MonoBehaviour
{
    [Header("Navigation")]
    public Button backButton;
    public Button logInButton;
    public Button forgotPasswordButton;

    [Header("Input Fields")]
    public TMP_InputField emailPhoneInput;
    public TMP_InputField passwordInput;

    [Header("Password Toggle")]
    public Button togglePasswordButton;
    public Image eyeIconImage;
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    [Header("Server Configuration")]
    // IMPORTANT: Set this to a non-functional placeholder in script.
    // The ACTUAL URL MUST be set in the Unity Inspector.
    public string loginUrl = "REPLACE_IN_INSPECTOR_LOGIN_URL";

    private bool isPasswordVisible = false;

    void Start()
    {
        if (backButton) backButton.onClick.AddListener(OnBackClicked);
        else Debug.LogError("BackButton not assigned in SignIn2Controller.", this);

        if (logInButton) logInButton.onClick.AddListener(OnLogInClicked);
        else Debug.LogError("LogInButton not assigned in SignIn2Controller.", this);

        if (forgotPasswordButton) forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        // else Debug.LogWarning("ForgotPasswordButton not assigned in SignIn2Controller.", this);

        if (togglePasswordButton)
        {
            togglePasswordButton.onClick.AddListener(TogglePasswordVisibility);
            UpdatePasswordVisibility();
        }
        else Debug.LogError("TogglePasswordButton not assigned in SignIn2Controller.", this);

        if (eyeIconImage == null || eyeOpenSprite == null || eyeClosedSprite == null)
            Debug.LogError("Eye icon sprites/image not assigned in SignIn2Controller.", this);
    }

    void OnBackClicked()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowSignIn1Screen();
    }

    void OnLogInClicked()
    {
        string loginIdentifier = emailPhoneInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(loginIdentifier) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Login Identifier and Password are required.", this);
            // TODO: Show UI error message
            return;
        }

        if (loginUrl == "REPLACE_IN_INSPECTOR_LOGIN_URL" || string.IsNullOrEmpty(loginUrl))
        {
            Debug.LogError("!!! CRITICAL: loginUrl is not set correctly in the SignIn2Controller Inspector. Please update it with your actual PHP script URL. !!!", this);
            // TODO: Show UI error message
            return;
        }

        StartCoroutine(LoginUserCoroutine(loginIdentifier, password));
    }

    IEnumerator LoginUserCoroutine(string loginIdentifier, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("loginIdentifier", loginIdentifier);
        form.AddField("password", password);

        Debug.Log($"Attempting to log in: id={loginIdentifier}, p=*** to URL: {loginUrl}");

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Login Network Error: {www.error}. URL: {www.url}", this);
                if (www.downloadHandler != null) Debug.LogError("Response: " + www.downloadHandler.text, this);
                // TODO: Show UI error message
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Login Server Response: " + responseText, this);
                if (responseText.Trim().StartsWith("Success"))
                {
                    Debug.Log("Login successful!", this);
                    if (UIManager.Instance != null) UIManager.Instance.LoadHomeScreen(); // Loads the "Home" scene
                }
                else
                {
                    Debug.LogError("Login failed: " + responseText, this);
                    // TODO: Show UI error message (e.g., "Invalid credentials")
                }
            }
        }
    }

    void OnForgotPasswordClicked()
    {
        Debug.Log("Forgot Password clicked - Implement functionality here.", this);
    }

    void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;
        UpdatePasswordVisibility();
    }

    void UpdatePasswordVisibility()
    {
        if (passwordInput == null || eyeIconImage == null) return;

        if (isPasswordVisible)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            if (eyeOpenSprite) eyeIconImage.sprite = eyeOpenSprite;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            if (eyeClosedSprite) eyeIconImage.sprite = eyeClosedSprite;
        }
        if (Application.isPlaying) passwordInput.ForceLabelUpdate();
    }
}