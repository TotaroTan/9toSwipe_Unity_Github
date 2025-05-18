// ButtonClickHandler.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Test : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private string targetSceneIfNotLoggedIn;
    [SerializeField] private string targetSceneIfLoggedIn;

    // ... (other fields if needed) ...

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogError($"Button component not found on {gameObject.name}");
        }
    }

    // This is the method that uses the AuthManager
    private bool IsUserLoggedIn()
    {
        // Access the AuthManager's instance and its public property
        if (AuthManager.Instance != null) // Good practice to check if instance exists
        {
            return AuthManager.Instance.IsUserLoggedIn;
        }
        else
        {
            Debug.LogError("AuthManager instance is not available in the scene! Cannot check login status.");
            return false; // Default to not logged in if manager is missing
        }
    }

    void HandleClick()
    {
        bool isLoggedIn = IsUserLoggedIn();

        if (isLoggedIn)
        {
            if (!string.IsNullOrEmpty(targetSceneIfLoggedIn))
            {
                Debug.Log($"{gameObject.name} clicked (User Logged In), loading scene: {targetSceneIfLoggedIn}");
                SceneLoader.LoadScene(targetSceneIfLoggedIn); // Assuming you have SceneLoader.cs
            }
            else
            {
                Debug.LogWarning($"ButtonClickHandler on {gameObject.name} (User Logged In) has no targetSceneIfLoggedIn set.");
                // Perform other logged-in action if any
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(targetSceneIfNotLoggedIn))
            {
                Debug.Log($"{gameObject.name} clicked (User NOT Logged In), loading scene: {targetSceneIfNotLoggedIn}");
                SceneLoader.LoadScene(targetSceneIfNotLoggedIn);
            }
            else
            {
                Debug.LogWarning($"ButtonClickHandler on {gameObject.name} (User NOT Logged In) has no targetSceneIfNotLoggedIn set.");
                // Perform other not-logged-in action if any
            }
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}