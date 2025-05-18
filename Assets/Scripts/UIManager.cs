using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen Panels (in Login Scene)")]
    public GameObject signIn1Panel;
    public GameObject registerPanel;
    public GameObject signIn2Panel;
    // homePanel is no longer a UI Panel here, it's a separate scene.

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: Only if UIManager needs to persist across scenes.
                                           // For this simple login->home flow, it's likely not needed.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ShowSignIn1Screen();
    }

    private void HideAllLoginScenePanels() // Renamed for clarity
    {
        if (signIn1Panel) signIn1Panel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);
        if (signIn2Panel) signIn2Panel.SetActive(false);
    }

    public void ShowSignIn1Screen()
    {
        HideAllLoginScenePanels();
        if (signIn1Panel) signIn1Panel.SetActive(true);
    }

    public void ShowRegisterScreen()
    {
        HideAllLoginScenePanels();
        if (registerPanel) registerPanel.SetActive(true);
    }

    public void ShowSignIn2Screen()
    {
        HideAllLoginScenePanels();
        if (signIn2Panel) signIn2Panel.SetActive(true);
    }

    public void LoadHomeScreen()
    {
        HideAllLoginScenePanels(); // Hide UI in current scene before loading new one
        SceneManager.LoadScene("Home main"); // Ensure "Home" scene is in Build Settings
        Debug.Log("Loading 'Home main' scene...");
    }

    public void LoadLoginScene(string sceneName = "MainLoginScene") // Or whatever your login scene is named
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log($"Loading {sceneName} scene...");
    }
}