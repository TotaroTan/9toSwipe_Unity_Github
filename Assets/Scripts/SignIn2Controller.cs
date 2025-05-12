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
    public TMP_InputField emailPhoneInput; // Will be used as 'loginIdentifier' for PHP
    public TMP_InputField passwordInput;

    [Header("Password Toggle")]
    public Button togglePasswordButton;
    public Image eyeIconImage;
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

    [Header("Server Configuration")]
    // !!! IMPORTANT: Replace with your actual URL !!!
    public string loginUrl = "http://YOUR_DOMAIN_PLACEHOLDER/api/login.php";


    private bool isPasswordVisible = false;

    void Start()
    {
        if (backButton) backButton.onClick.AddListener(OnBackClicked);
        else Debug.LogError("BackButton not assigned in SignIn2Controller.");

        if (logInButton) logInButton.onClick.AddListener(OnLogInClicked);
        else Debug.LogError("LogInButton not assigned in SignIn2Controller.");

        if (forgotPasswordButton) forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        // else Debug.LogWarning("ForgotPasswordButton not assigned in SignIn2Controller."); // Optional

        if (togglePasswordButton)
        {
            togglePasswordButton.onClick.AddListener(TogglePasswordVisibility);
            UpdatePasswordVisibility(); // Set initial state
        }
        else
        {
            Debug.LogError("TogglePasswordButton not assigned in SignIn2Controller.");
        }

        if (eyeIconImage == null || eyeOpenSprite == null || eyeClosedSprite == null)
        {
            Debug.LogError("One or more eye icon sprites/image not assigned in SignIn2Controller.");
        }
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
            Debug.LogError("Login Identifier and Password are required.");
            // TODO: Show UI error message
            return;
        }
        if (loginUrl == "http://YOUR_DOMAIN_PLACEHOLDER/api/login.php")
        {
            Debug.LogError("!!! CRITICAL: loginUrl is not set in SignIn2Controller. Please update it in the Inspector. !!!");
            return;
        }

        StartCoroutine(LoginUserCoroutine(loginIdentifier, password));
    }

    IEnumerator LoginUserCoroutine(string loginIdentifier, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("loginIdentifier", loginIdentifier);
        form.AddField("password", password);

        Debug.Log($"Attempting to log in: id={loginIdentifier}, p=***");

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Login Network Error: " + www.error);
                 if (www.downloadHandler != null) Debug.LogError("Response: " + www.downloadHandler.text);
                // TODO: Show UI error message
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Login Server Response: " + responseText);
                if (responseText.Trim().StartsWith("Success")) // Trim to handle potential whitespace
                {
                    Debug.Log("Login successful!");
                    // Example: Parse UserID and Username if PHP sends them back
                    // string[] responseParts = responseText.Split(',');
                    // if (responseParts.Length >= 2) {
                    //     string userId = responseParts[0].Split(':')[1].Trim();
                    //     string username = responseParts[1].Split(':')[1].Trim();
                    //     Debug.Log($"Logged in UserID: {userId}, Username: {username}");
                    //     // Store these if needed (e.g., for subsequent API calls)
                    // }
                    if (UIManager.Instance != null) UIManager.Instance.ShowHomeScreen();
                }
                else
                {
                    Debug.LogError("Login failed: " + responseText);
                    // TODO: Show UI error message (e.g., "Invalid credentials")
                }
            }
        }
    }


    void OnForgotPasswordClicked()
    {
        Debug.Log("Forgot Password clicked - Implement functionality here.");
        // Potentially show another panel or link to a web page
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
        passwordInput.ForceLabelUpdate();
    }
}