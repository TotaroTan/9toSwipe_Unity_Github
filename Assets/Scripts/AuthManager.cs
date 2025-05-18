// AuthManager.cs
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    // --- Singleton Instance ---
    private static AuthManager _instance;
    public static AuthManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AuthManager>(); // Try to find existing instance
                if (_instance == null)
                {
                    // If no instance exists, create one (optional, or log an error)
                    GameObject singletonObject = new GameObject("AuthManager_Runtime");
                    _instance = singletonObject.AddComponent<AuthManager>();
                    Debug.LogWarning("AuthManager instance not found, created one. It's better to place it in your scene manually.");
                }
            }
            return _instance;
        }
    }
    // --- End Singleton Instance ---

    // --- Public property to check login state ---
    public bool IsUserLoggedIn { get; private set; } = false;

    // You might also store user details here after login
    // public string LoggedInUserID { get; private set; }
    // public string LoggedInUserEmail { get; private set; }

    void Awake()
    {
        // Ensure only one instance of the AuthManager exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Make AuthManager persist across scene loads

            // Optional: Load initial login state from PlayerPrefs or secure storage
            // This makes the user stay "logged in" if they close and reopen the app
            LoadLoginState();
        }
        else if (_instance != this)
        {
            Debug.LogWarning("Another instance of AuthManager found. Destroying this duplicate.");
            Destroy(gameObject);
        }
    }

    private void LoadLoginState()
    {
        // Example: Check if a session indicator exists
        if (PlayerPrefs.HasKey("UserSessionActive") && PlayerPrefs.GetInt("UserSessionActive") == 1)
        {
            // Potentially also load UserID, username etc. from PlayerPrefs
            // string userId = PlayerPrefs.GetString("UserID", "");
            // if (!string.IsNullOrEmpty(userId)) {
            //     LoggedInUserID = userId;
            //     IsUserLoggedIn = true;
            //     Debug.Log("Restored logged-in state for user: " + userId);
            // }
            IsUserLoggedIn = true; // Simplified for now
            Debug.Log("Restored logged-in state.");
        }
    }

    // --- Public methods to be called by your login/logout UI/logic ---
    public void HandleLoginSuccess(string userId, string userEmail /*, string authToken, etc. */)
    {
        IsUserLoggedIn = true;
        // LoggedInUserID = userId;
        // LoggedInUserEmail = userEmail;
        Debug.Log($"User '{userId}' logged in successfully.");

        // Persist login state (simple example)
        PlayerPrefs.SetInt("UserSessionActive", 1);
        // PlayerPrefs.SetString("UserID", userId);
        PlayerPrefs.Save();
    }

    public void HandleLogout()
    {
        IsUserLoggedIn = false;
        // LoggedInUserID = null;
        // LoggedInUserEmail = null;
        Debug.Log("User logged out.");

        // Clear persisted login state
        PlayerPrefs.DeleteKey("UserSessionActive");
        // PlayerPrefs.DeleteKey("UserID");
        PlayerPrefs.Save();
    }

    // You would have other methods here for:
    // - Initiating login with email/password
    // - Initiating registration
    // - Handling forgot password, etc.
    // These would typically involve making API calls to your backend.
}