// SignIn2Controller.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
// IMPORTANT: Add your SceneLoader if it's in a different namespace
// using YourNamespace; // If SceneLoader is in a namespace

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
    public string loginUrl = "REPLACE_IN_INSPECTOR_LOGIN_URL";

    private bool isPasswordVisible = false;

    void Start()
    {
        // Your existing Start() method...
        if (backButton) backButton.onClick.AddListener(OnBackClicked);
        else Debug.LogError("BackButton not assigned in SignIn2Controller.", this);

        if (logInButton) logInButton.onClick.AddListener(OnLogInClicked);
        else Debug.LogError("LogInButton not assigned in SignIn2Controller.", this);

        if (forgotPasswordButton) forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        // else Debug.LogWarning("ForgotPasswordButton not assigned in SignIn2Controller.", this);

        if (togglePasswordButton)
        {
            togglePasswordButton.onClick.AddListener(TogglePasswordVisibility);
            UpdatePasswordVisibility(); // Call this to set initial state
        }
        else Debug.LogError("TogglePasswordButton not assigned in SignIn2Controller.", this);

        if (eyeIconImage == null || eyeOpenSprite == null || eyeClosedSprite == null)
            Debug.LogError("Eye icon sprites/image not assigned in SignIn2Controller.", this);
    }

    void OnBackClicked()
    {
        // Assuming UIManager is a singleton or accessible
        if (UIManager.Instance != null) UIManager.Instance.ShowSignIn1Screen();
        else Debug.LogError("UIManager instance not found for back navigation.");
    }

    void OnLogInClicked()
    {
        string loginIdentifier = emailPhoneInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(loginIdentifier) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Login Identifier and Password are required.", this);
            // TODO: Show UI error message (e.g., set a feedback text)
            return;
        }

        if (loginUrl == "REPLACE_IN_INSPECTOR_LOGIN_URL" || string.IsNullOrEmpty(loginUrl))
        {
            Debug.LogError("!!! CRITICAL: loginUrl is not set correctly in the SignIn2Controller Inspector. Please update it with your actual PHP script URL. !!!", this);
            // TODO: Show UI error message
            return;
        }

        logInButton.interactable = false; // Disable button during request
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

            logInButton.interactable = true; // Re-enable button after request is done

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Login Network Error: {www.error}. URL: {www.url}", this);
                if (www.downloadHandler != null) Debug.LogError("Response: " + www.downloadHandler.text, this);
                // TODO: Show UI error message (e.g., "Network error, please try again.")
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Login Server Response: " + responseText, this);

                // You'll need to parse your server's response more robustly.
                // This is a very basic check.
                // Your server should ideally return JSON with a clear success/error status and user data.
                if (IsLoginSuccessful(responseText)) // Replace with your actual success check logic
                {
                    Debug.Log("Login successful based on server response!", this);

                    // ---!!! CONNECTION TO AUTHMANAGER HAPPENS HERE !!!---
                    if (AuthManager.Instance != null)
                    {
                        // Extract user data from responseText if your server sends it back
                        // For example, if your server sends back: {"status":"Success", "userId":"123", "email":"user@example.com"}
                        string userId = ParseUserIdFromResponse(responseText); // Implement this helper
                        string userEmail = ParseUserEmailFromResponse(responseText); // Implement this helper

                        AuthManager.Instance.HandleLoginSuccess(userId, userEmail /*, other data like token */);
                        Debug.Log("AuthManager updated with successful login.");
                    }
                    else
                    {
                        Debug.LogError("AuthManager instance not found! Cannot finalize login state.");
                        // TODO: Show critical system error UI
                        yield break; // Stop further execution in this coroutine
                    }
                    // ---!!! END CONNECTION TO AUTHMANAGER !!!---

                    // Now navigate. UIManager.Instance.LoadHomeScreen() might be your SceneLoader.LoadScene("Home")
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.LoadHomeScreen(); // Assuming this loads your "Home" scene
                    }
                    else
                    {
                        Debug.LogWarning("UIManager.Instance not found, attempting direct scene load to 'Home'.");
                        SceneLoader.LoadScene("Home"); // Fallback or direct call
                    }
                }
                else
                {
                    Debug.LogError("Login failed based on server response: " + responseText, this);
                    // TODO: Show UI error message (e.g., "Invalid credentials" or the error from responseText)
                }
            }
        }
    }

    // --- Helper methods for parsing response (YOU NEED TO IMPLEMENT THESE BASED ON YOUR SERVER'S RESPONSE FORMAT) ---
    private bool IsLoginSuccessful(string responseText)
    {
        // EXAMPLE: Very basic check. Your server should provide a clear success indicator.
        // If your server returns JSON like {"status":"Success", ...}
        // you would parse the JSON here. For now, a simple string check.
        return responseText.Trim().ToLower().Contains("\"status\":\"success\"") || responseText.Trim().ToLower().StartsWith("success");
        // IMPORTANT: Make this robust based on your actual server response!
    }

    private string ParseUserIdFromResponse(string responseText)
    {
        // EXAMPLE: If your server returns JSON: {"userId":"123", ...}
        // You would use a JSON parser (like SimpleJSON or Unity's JsonUtility)
        // For a very simple string extraction (not recommended for complex JSON):
        if (responseText.Contains("\"userId\":\""))
        {
            int startIndex = responseText.IndexOf("\"userId\":\"") + "\"userId\":\"".Length;
            int endIndex = responseText.IndexOf("\"", startIndex);
            if (endIndex > startIndex)
            {
                return responseText.Substring(startIndex, endIndex - startIndex);
            }
        }
        return "UnknownUser"; // Default or error value
    }

    private string ParseUserEmailFromResponse(string responseText)
    {
        // EXAMPLE: If your server returns JSON: {"email":"user@example.com", ...}
        if (responseText.Contains("\"email\":\""))
        {
            int startIndex = responseText.IndexOf("\"email\":\"") + "\"email\":\"".Length;
            int endIndex = responseText.IndexOf("\"", startIndex);
            if (endIndex > startIndex)
            {
                return responseText.Substring(startIndex, endIndex - startIndex);
            }
        }
        // If email isn't directly in login response, you might just use the input email or leave it blank
        return emailPhoneInput.text; // Or fetch from response if available
    }
    // --- End Helper methods ---


    void OnForgotPasswordClicked()
    {
        Debug.Log("Forgot Password clicked - Implement functionality here.", this);
        // TODO: Navigate to Forgot Password scene or show a panel
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
        // Refresh the input field to show/hide password characters
        // For TMP_InputField, it usually updates automatically, but if not:
        if (Application.isPlaying)
        {
            passwordInput.ForceLabelUpdate();
            // A common trick if ForceLabelUpdate isn't enough:
            // string currentText = passwordInput.text;
            // passwordInput.text = " "; // Temporarily change text
            // passwordInput.text = currentText; // Set it back
            // passwordInput.Select(); // Reselect
            // passwordInput.ActivateInputField(); // Reactivate
        }
    }
}