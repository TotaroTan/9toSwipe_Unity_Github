// NavBarManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Still needed if SOME nav buttons go to other scenes

public class NavBarManager : MonoBehaviour
{
    public static NavBarManager Instance { get; private set; } // Optional: useful if other scripts need to interact

    [Header("Navigation Buttons")]
    public Button homeButton;
    public Button searchButton;
    public Button wishlistButton;
    public Button appliedButton;
    public Button profileButton;

    [Header("Content Canvases (Assign in Inspector)")]
    public GameObject wishlistCanvasGO;
    public GameObject appliedCanvasGO;
    // Add other content canvases here if they are part of this same-scene tab system
    // public GameObject homeCanvasGO;
    // public GameObject searchCanvasGO;
    // public GameObject profileCanvasGO;


    [Header("Active Button Visuals (Optional)")]
    public Color activeButtonColor = Color.yellow; // Example
    public Color inactiveButtonColor = Color.white; // Example

    // --- SCENE NAMES (For buttons that DO navigate to different scenes) ---
    // Adjust these if some buttons still load completely different scenes
    public string homeSceneName = "HomeScene";
    public string searchSceneName = "SearchScene";
    // Wishlist and Applied are handled by canvas switching now
    public string profileSceneName = "UserProfileScene";


    // Enum to track active canvas for highlighting (optional, but good for clarity)
    private enum ActiveCanvas { None, Wishlist, Applied, Home, Search, Profile }
    private ActiveCanvas currentActiveCanvasKey;

    void Awake()
    {
        // Singleton pattern - useful if other scripts might need to trigger navigation
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad needed if this NavBar is specific to this one scene.
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        // Null checks for canvases
        if (wishlistCanvasGO == null) Debug.LogError("NavBarManager: WishlistCanvasGO not assigned!");
        if (appliedCanvasGO == null) Debug.LogError("NavBarManager: AppliedCanvasGO not assigned!");

        SetupButtonListeners();
    }

    void Start()
    {
        // Set an initial active canvas, e.g., Wishlist
        ShowWishlistCanvas();
    }

    void SetupButtonListeners()
    {
        // Buttons that switch canvases within this scene
        if (wishlistButton) wishlistButton.onClick.AddListener(ShowWishlistCanvas);
        if (appliedButton) appliedButton.onClick.AddListener(ShowAppliedCanvas);

        // Buttons that might navigate to different scenes (if any)
        if (homeButton) homeButton.onClick.AddListener(() => NavigateToScene(homeSceneName));
        if (searchButton) searchButton.onClick.AddListener(() => NavigateToScene(searchSceneName));
        if (profileButton) profileButton.onClick.AddListener(() => NavigateToScene(profileSceneName));

        // Example if Home/Search/Profile were also canvases in this scene:
        // if (homeButton) homeButton.onClick.AddListener(ShowHomeCanvas);
        // if (searchButton) searchButton.onClick.AddListener(ShowSearchCanvas);
        // if (profileButton) profileButton.onClick.AddListener(ShowProfileCanvas);
    }

    // --- Methods to show specific content canvases ---

    public void ShowWishlistCanvas()
    {
        Debug.Log("NavBar: Showing Wishlist Canvas");
        SetCanvasActive(wishlistCanvasGO, true);
        SetCanvasActive(appliedCanvasGO, false);
        // SetCanvasActive(homeCanvasGO, false); // Deactivate others
        // SetCanvasActive(searchCanvasGO, false);
        // SetCanvasActive(profileCanvasGO, false);

        currentActiveCanvasKey = ActiveCanvas.Wishlist;
        UpdateButtonHighlights();
    }

    public void ShowAppliedCanvas()
    {
        Debug.Log("NavBar: Showing Applied Canvas");
        SetCanvasActive(wishlistCanvasGO, false);
        SetCanvasActive(appliedCanvasGO, true);
        // SetCanvasActive(homeCanvasGO, false); // Deactivate others
        // SetCanvasActive(searchCanvasGO, false);
        // SetCanvasActive(profileCanvasGO, false);

        currentActiveCanvasKey = ActiveCanvas.Applied;
        UpdateButtonHighlights();
    }

    // Add similar ShowXYZCanvas() methods if Home, Search, Profile are also canvases in this scene

    private void SetCanvasActive(GameObject canvasGO, bool isActive)
    {
        if (canvasGO != null)
        {
            canvasGO.SetActive(isActive);
        }
        else
        {
            // Only log error if trying to activate a null canvas, deactivating null is fine.
            if(isActive) Debug.LogWarning($"NavBarManager: Tried to activate a null canvas GameObject.");
        }
    }


    // --- Method for buttons that load completely different scenes ---
    public void NavigateToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("NavBarManager: Scene name is empty for NavigateToScene!");
            return;
        }

        Debug.Log($"NavBar: Navigating to scene: {sceneName}");
        // If you have a SceneFader, use it here
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneAsyncWithFade(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
        // Note: When a new scene loads, this NavBar (if not DontDestroyOnLoad) will be gone.
        // Highlighting for buttons leading to *other scenes* is tricky unless the NavBar persists.
        // For now, this assumes highlights are primarily for the canvas tabs in *this* scene.
    }

    public void UpdateButtonHighlights()
    {
        // Debug.Log($"NavBar: Updating highlights for current canvas: {currentActiveCanvasKey}");
        HighlightButton(wishlistButton, currentActiveCanvasKey == ActiveCanvas.Wishlist);
        HighlightButton(appliedButton, currentActiveCanvasKey == ActiveCanvas.Applied);

        // For buttons leading to other scenes, they'd generally be "inactive" in this context,
        // unless you have a more complex state system.
        HighlightButton(homeButton, currentActiveCanvasKey == ActiveCanvas.Home); // Or always false if Home is a different scene
        HighlightButton(searchButton, currentActiveCanvasKey == ActiveCanvas.Search); // Or always false
        HighlightButton(profileButton, currentActiveCanvasKey == ActiveCanvas.Profile); // Or always false
    }

    void HighlightButton(Button button, bool isActive)
    {
        if (button == null) return;

        // Option 1: Change button's target graphic color (e.g., Image background)
        var targetImage = button.targetGraphic as Image;
        if (targetImage != null)
        {
            targetImage.color = isActive ? activeButtonColor : inactiveButtonColor;
        }

        // Option 2: Change button's TextMeshPro text color
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = isActive ? activeButtonColor : inactiveButtonColor; // Ensure colors are suitable for text
        }

        // Option 3: Enable/disable an "indicator" GameObject child of the button
        // Transform activeIndicator = button.transform.Find("ActiveIndicatorImage");
        // if (activeIndicator != null) activeIndicator.gameObject.SetActive(isActive);

        // Option 4: Make the active button non-interactable
        button.interactable = !isActive;
    }
}