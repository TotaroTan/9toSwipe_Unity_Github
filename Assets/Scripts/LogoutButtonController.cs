// LogoutButtonController.cs
using UnityEngine;
using UnityEngine.UI; // Required for Button
// using YourNamespace; // If SceneLoader is in a different namespace

[RequireComponent(typeof(Button))]
public class LogoutButtonController : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("The name of the scene to load after the user logs out.")]
    // ---!!! THIS LINE WAS MISSING [SerializeField] AND public !!!---
    [SerializeField] public string targetSceneAfterLogout = "LoginScene"; // Default to a common logout destination

    private Button logoutButton;

    void Awake()
    {
        logoutButton = GetComponent<Button>();
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(HandleLogoutClicked);
        }
        else
        {
            Debug.LogError($"LogoutButtonController: Button component not found on {gameObject.name}", this);
        }
    }

    void HandleLogoutClicked()
    {
        Debug.Log($"LogoutButtonController: Logout button clicked on {gameObject.name}.");

        // 1. Call the AuthManager to handle the logout logic
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.HandleLogout(); // This will set IsUserLoggedIn to false and clear PlayerPrefs
            Debug.Log("LogoutButtonController: AuthManager.HandleLogout() called.");
        }
        else
        {
            Debug.LogError("LogoutButtonController: AuthManager instance is not available! Cannot perform logout.", this);
        }

        // 2. Navigate to the target scene
        if (!string.IsNullOrEmpty(targetSceneAfterLogout))
        {
            Debug.Log($"LogoutButtonController: Navigating to '{targetSceneAfterLogout}' after logout.");
            try
            {
                SceneLoader.LoadScene(targetSceneAfterLogout); // Assuming you have SceneLoader.cs
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"LogoutButtonController: Error loading scene '{targetSceneAfterLogout}': {ex.Message}\n{ex.StackTrace}", this);
            }
        }
        else
        {
            Debug.LogWarning($"LogoutButtonController: targetSceneAfterLogout is not set on {gameObject.name}. No navigation will occur after logout.", this);
        }
    }

    void OnDestroy()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(HandleLogoutClicked);
        }
    }
}