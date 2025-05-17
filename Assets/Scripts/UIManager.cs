using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen Panels")]
    public GameObject signIn1Panel;
    public GameObject registerPanel;
    public GameObject signIn2Panel;
    // public GameObject homePanel; // We might not need this if we're switching scenes

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // If UIManager needs to persist across scenes (e.g., to show a loading screen
            // or manage global UI elements not tied to a specific game scene),
            // then uncomment DontDestroyOnLoad. Otherwise, it can be destroyed
            // when a new scene loads if it's not needed in the "Home" scene.
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Start with the SignIn1 screen
        ShowSignIn1Screen();
    }

    private void HideAllScreens()
    {
        // This will only hide panels within the current (login) scene
        if (signIn1Panel) signIn1Panel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);
        if (signIn2Panel) signIn2Panel.SetActive(false);
        // if (homePanel) homePanel.SetActive(false); // No longer directly managing homePanel this way
    }

    public void ShowSignIn1Screen()
    {
        HideAllScreens();
        if (signIn1Panel) signIn1Panel.SetActive(true);
    }

    public void ShowRegisterScreen()
    {
        HideAllScreens();
        if (registerPanel) registerPanel.SetActive(true);
    }

    public void ShowSignIn2Screen()
    {
        HideAllScreens();
        if (signIn2Panel) signIn2Panel.SetActive(true);
    }

    public void LoadHomeScreen() // Renamed for clarity
    {
        // Hide all UI panels in the current scene before loading the new one
        HideAllScreens();

        // Load the "Home" scene
        // Make sure the scene name "Home" exactly matches the name of your scene file
        // and that it's added to Build Settings.
        SceneManager.LoadScene("Home");
        Debug.Log("Loading Home scene...");
    }

    // If you still want a method to show a HomePanel *within the login scene* for some reason,
    // you can keep the old ShowHomeScreen() method and call it when appropriate.
    // But for redirecting to a new scene, LoadHomeScreen() is what you need.
}