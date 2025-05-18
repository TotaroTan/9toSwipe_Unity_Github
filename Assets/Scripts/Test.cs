// Test.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Test : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private string targetSceneIfNotLoggedIn;
    [SerializeField] private string targetSceneIfLoggedIn;

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
            Debug.LogError($"Button component not found on {gameObject.name}", this);
        }
    }

    private bool IsUserLoggedIn()
    {
        if (AuthManager.Instance == null)
        {
            Debug.LogError($"Test.cs ({gameObject.name}) - AuthManager.Instance IS NULL! Cannot determine login status.", this);
            return false;
        }
        bool status = AuthManager.Instance.IsUserLoggedIn;
        //Debug.Log($"Test.cs ({gameObject.name}) - IsUserLoggedIn() check: AuthManager.Instance.IsUserLoggedIn = {status}", this);
        return status;
    }

    void HandleClick()
    {
        Debug.Log($"Test.cs - HandleClick invoked for button: '{gameObject.name}'", this);

        bool isLoggedIn = IsUserLoggedIn();
        Debug.Log($"Test.cs - Login status determined for '{gameObject.name}': {isLoggedIn}", this);

        if (isLoggedIn)
        {
            if (!string.IsNullOrEmpty(targetSceneIfLoggedIn))
            {
                Debug.Log($"Test.cs - User IS logged in for '{gameObject.name}'. Attempting to load scene from 'targetSceneIfLoggedIn': '{targetSceneIfLoggedIn}'", this);
                SceneLoader.LoadScene(targetSceneIfLoggedIn);
            }
            else
            {
                Debug.LogWarning($"Test.cs - User IS logged in, but targetSceneIfLoggedIn is not set for button: '{gameObject.name}'", this);
            }
        }
        else // User is NOT logged in
        {
            Debug.Log($"Test.cs - User is NOT logged in for '{gameObject.name}'. Will attempt to load 'targetSceneIfNotLoggedIn' which is set to: '{targetSceneIfNotLoggedIn}'", this);

            if (!string.IsNullOrEmpty(targetSceneIfNotLoggedIn))
            {
                // --- RIGOROUS CHECK ---
                string sceneToActuallyLoad = targetSceneIfNotLoggedIn.Trim(); // Trim whitespace just in case
                Debug.Log($"Test.cs - Scene name from Inspector (after trim): '{sceneToActuallyLoad}'", this);
                Debug.Log($"Test.cs - Hardcoded comparison: Is it 'Home'? {(sceneToActuallyLoad == "Home")}", this);


                // --- TEMPORARY DIRECT TEST ---
                // Uncomment the next line and comment out the one after to force loading "Home"
                // if you are absolutely sure "Home" is the correct name and it's in build settings.
                // This bypasses the inspector field for this specific test.

                // string sceneToForce = "Home";
                // Debug.Log($"Test.cs - FORCING LOAD OF '{sceneToForce}' for NOT LOGGED IN state for testing purposes.");
                // SceneLoader.LoadScene(sceneToForce);

                // Original line:
                SceneLoader.LoadScene(sceneToActuallyLoad);
            }
            else
            {
                Debug.LogWarning($"Test.cs - User is NOT logged in, and targetSceneIfNotLoggedIn is not set for button: '{gameObject.name}'", this);
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