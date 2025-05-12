using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro InputField
using System.Collections; // Required for IEnumerator
using UnityEngine.Networking; // Required for UnityWebRequest

public class RegisterController : MonoBehaviour
{
    [Header("Navigation Buttons")]
    public Button backButton;
    public Button createAccountButton;
    public Button alreadyHaveAccountButton;

    [Header("Input Fields")]
    public TMP_InputField emailPhoneInput; // Using this as 'email' for the PHP script
    public TMP_InputField displayNameInput;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField dateOfBirthInput; // Ensure your PHP script can handle/parse this

    [Header("Password Toggle")]
    public Button togglePasswordButton;
    public Image eyeIconImage; // The Image component on TogglePasswordButton
    public Sprite eyeOpenSprite; // Assign your 'eye.png' Sprite in Inspector
    public Sprite eyeClosedSprite; // Assign your 'eye-close.png' Sprite in Inspector

    [Header("Server Configuration")]
    // !!! IMPORTANT: Replace with your actual URL !!!
    public string registerUrl = "http://YOUR_DOMAIN_PLACEHOLDER/api/register.php";

    private bool isPasswordVisible = false;

    void Start()
    {
        // Navigation
        if (backButton) backButton.onClick.AddListener(OnBackClicked);
        else Debug.LogError("BackButton not assigned in RegisterController.");

        if (createAccountButton) createAccountButton.onClick.AddListener(OnCreateAccountClicked);
        else Debug.LogError("CreateAccountButton not assigned in RegisterController.");

        if (alreadyHaveAccountButton) alreadyHaveAccountButton.onClick.AddListener(OnAlreadyHaveAccountClicked);
        else Debug.LogError("AlreadyHaveAccountButton not assigned in RegisterController.");

        // Password Toggle
        if (togglePasswordButton)
        {
            togglePasswordButton.onClick.AddListener(TogglePasswordVisibility);
            UpdatePasswordVisibility(); // Set initial state
        }
        else
        {
            Debug.LogError("TogglePasswordButton not assigned in RegisterController.");
        }

        if (eyeIconImage == null || eyeOpenSprite == null || eyeClosedSprite == null)
        {
            Debug.LogError("One or more eye icon sprites/image not assigned in RegisterController.");
        }
    }

    void OnBackClicked()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowSignIn1Screen();
    }

    void OnCreateAccountClicked()
    {
        string email = emailPhoneInput.text;
        string displayName = displayNameInput.text;
        string username = usernameInput.text;
        string password = passwordInput.text;
        string dob = dateOfBirthInput.text;

        // Basic client-side validation (prototype)
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
        {
            Debug.LogError("Username, Password, and Email are required for registration.");
            // TODO: Show UI error message to user (e.g., via a TextMeshPro field)
            return;
        }
        if (registerUrl == "http://YOUR_DOMAIN_PLACEHOLDER/api/register.php" || string.IsNullOrEmpty(registerUrl)) // Added IsNullOrEmpty for safety
    {
        Debug.LogError("!!! CRITICAL: registerUrl is not set correctly in the RegisterController Inspector. Please update it. !!!");
        return;
        }

        StartCoroutine(RegisterUserCoroutine(username, password, email, displayName, dob));
    }

    IEnumerator RegisterUserCoroutine(string username, string password, string email, string displayName, string dob)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("email", email);
        // Add other fields to the form if your PHP script handles them:
        // form.AddField("displayName", displayName);
        // form.AddField("dateOfBirth", dob); // Ensure PHP uses 'dateOfBirth' if you send this

        Debug.Log($"Attempting to register: u={username}, p=***, e={email}");

        using (UnityWebRequest www = UnityWebRequest.Post(registerUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Registration Network Error: " + www.error);
                if (www.downloadHandler != null) Debug.LogError("Response: " + www.downloadHandler.text);
                // TODO: Show UI error message to user
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Registration Server Response: " + responseText);
                if (responseText.Trim().StartsWith("Success")) // Trim to handle potential whitespace
                {
                    Debug.Log("Registration successful!");
                    // Optionally, you can directly log them in or go to the login screen
                    if (UIManager.Instance != null) UIManager.Instance.ShowSignIn2Screen();
                }
                else
                {
                    Debug.LogError("Registration failed: " + responseText);
                    // TODO: Show UI error message to user (e.g., "Username taken", "Email exists")
                }
            }
        }
    }

    void OnAlreadyHaveAccountClicked()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowSignIn1Screen();
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
        passwordInput.ForceLabelUpdate(); // Refresh the input field display
    }
}