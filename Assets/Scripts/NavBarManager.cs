// NavBarManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed if any buttons load different scenes
using TMPro;                     // Needed for TextMeshProUGUI button text manipulation

public class NavBarManager : MonoBehaviour
{
    public static NavBarManager Instance { get; private set; }

    [Header("Navigation Buttons (Assign in Inspector)")]
    public Button homeButton;
    public Button searchButton;
    public Button wishlistButton;
    public Button appliedButton;
    public Button profileButton;

    [Header("Content Canvases (Assign in Inspector)")]
    [Tooltip("The Canvas/GameObject for the Wishlist page content.")]
    public GameObject wishlistCanvasGO;
    [Tooltip("The Canvas/GameObject for the Applied Jobs page content.")]
    public GameObject appliedCanvasGO;
    // Example: If Home, Search, Profile are also canvases in THIS scene, add fields for them:
    // public GameObject homeCanvasGO;
    // public GameObject searchCanvasGO;
    // public GameObject profileCanvasGO;

    [Header("Scene Names (For buttons that load NEW scenes)")]
    [Tooltip("Name of the scene for the Home button, if it loads a new scene.")]
    public string homeSceneName = "HomeScene"; // Example
    [Tooltip("Name of the scene for the Search button, if it loads a new scene.")]
    public string searchSceneName = "SearchScene"; // Example
    [Tooltip("Name of the scene for the Profile button, if it loads a new scene.")]
    public string profileSceneName = "UserProfileScene"; // Example

    [Header("Active Button Visuals (Optional)")]
    public Color activeButtonColor = new Color32(255, 180, 0, 255); // Example: Orange
    public Color inactiveButtonColor = Color.white;
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = new Color32(80, 80, 80, 255);


    // Enum to track which "page" (canvas or scene context) is active for highlighting
    public enum ActiveContext { None, Wishlist, Applied, Home, Search, Profile } // <<< MADE PUBLIC
    private ActiveContext currentActiveContext;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad if this NavBar is specific to this scene's UI flow.
        }
        else if (Instance != this)
        {
            Debug.LogWarning("NavBarManager: Duplicate instance found. Destroying self.");
            Destroy(gameObject);
            return;
        }

        // Essential reference checks
        if (wishlistCanvasGO == null) Debug.LogError("NavBarManager: WishlistCanvasGO not assigned in the Inspector!");
        if (appliedCanvasGO == null) Debug.LogError("NavBarManager: AppliedCanvasGO not assigned in the Inspector!");
        // Add checks for other assigned canvases if you have them (e.g., homeCanvasGO)

        SetupButtonListeners();
    }

    void Start()
    {
        // Set an initial active canvas, e.g., Wishlist.
        // Change this if you want a different canvas to be active on start.
        ShowWishlistCanvas();
    }

    void SetupButtonListeners()
    {
        // Buttons that switch canvases within this scene
        if (wishlistButton) wishlistButton.onClick.AddListener(ShowWishlistCanvas);
        else Debug.LogWarning("NavBarManager: WishlistButton not assigned.");

        if (appliedButton) appliedButton.onClick.AddListener(ShowAppliedCanvas);
        else Debug.LogWarning("NavBarManager: AppliedButton not assigned.");

        // Buttons that might navigate to different scenes or other canvases in this scene
        if (homeButton) homeButton.onClick.AddListener(() => NavigateToSceneOrCanvas(ActiveContext.Home, homeSceneName));
        else Debug.LogWarning("NavBarManager: HomeButton not assigned.");

        if (searchButton) searchButton.onClick.AddListener(() => NavigateToSceneOrCanvas(ActiveContext.Search, searchSceneName));
        else Debug.LogWarning("NavBarManager: SearchButton not assigned.");

        if (profileButton) profileButton.onClick.AddListener(() => NavigateToSceneOrCanvas(ActiveContext.Profile, profileSceneName));
        else Debug.LogWarning("NavBarManager: ProfileButton not assigned.");
    }

    // --- Methods to show specific content canvases within THIS scene ---
    public void ShowWishlistCanvas()
    {
        // Debug.Log("NavBar: Activating Wishlist Canvas");
        SetSingleCanvasActive(wishlistCanvasGO);
        currentActiveContext = ActiveContext.Wishlist;
        UpdateButtonHighlights();
    }

    public void ShowAppliedCanvas()
    {
        // Debug.Log("NavBar: Activating Applied Canvas");
        SetSingleCanvasActive(appliedCanvasGO);
        currentActiveContext = ActiveContext.Applied;
        UpdateButtonHighlights();
    }

    // Example: If Home was also a canvas in this scene
    // public void ShowHomeCanvas()
    // {
    //     Debug.Log("NavBar: Activating Home Canvas");
    //     SetSingleCanvasActive(homeCanvasGO);
    //     currentActiveContext = ActiveContext.Home;
    //     UpdateButtonHighlights();
    // }


    // Helper to activate one canvas and deactivate others relevant to this tab system
    private void SetSingleCanvasActive(GameObject canvasToActivate)
    {
        // Deactivate all managed content canvases first
        if (wishlistCanvasGO != null) wishlistCanvasGO.SetActive(false);
        if (appliedCanvasGO != null) appliedCanvasGO.SetActive(false);
        // if (homeCanvasGO != null) homeCanvasGO.SetActive(false); // If Home is a canvas in this scene
        // if (searchCanvasGO != null) searchCanvasGO.SetActive(false); // If Search is a canvas
        // if (profileCanvasGO != null) profileCanvasGO.SetActive(false); // If Profile is a canvas

        // Activate the target canvas
        if (canvasToActivate != null)
        {
            canvasToActivate.SetActive(true);
        }
        else
        {
            Debug.LogError("NavBarManager: Attempted to activate a null canvas in SetSingleCanvasActive!");
        }
    }

    // --- Method for buttons that might load NEW scenes OR switch to other canvases in this scene ---
    public void NavigateToSceneOrCanvas(ActiveContext targetContext, string sceneNameIfExternal)
    {
        bool navigatedInternally = false;

        // --- CHECK FOR INTERNAL CANVAS NAVIGATION FIRST ---
        // Uncomment and adapt if Home, Search, Profile are canvases in this scene
        /*
        switch (targetContext)
        {
            case ActiveContext.Home:
                if (homeCanvasGO != null)
                {
                    ShowHomeCanvas(); // Assumes ShowHomeCanvas() exists and handles highlights
                    navigatedInternally = true;
                }
                break;
            case ActiveContext.Search:
                // if (searchCanvasGO != null) { ShowSearchCanvas(); navigatedInternally = true; }
                break;
            case ActiveContext.Profile:
                // if (profileCanvasGO != null) { ShowProfileCanvas(); navigatedInternally = true; }
                break;
            // Wishlist and Applied are handled by their direct button listeners, not this method.
        }
        */

        if (navigatedInternally)
        {
            return; // Handled by an internal canvas switch
        }

        // --- IF NOT HANDLED INTERNALLY, ATTEMPT TO LOAD EXTERNAL SCENE ---
        if (string.IsNullOrEmpty(sceneNameIfExternal))
        {
            Debug.LogWarning($"NavBarManager: No external scene name provided for context {targetContext}, and no internal canvas handler found.");
            // Optionally, set context to None and update highlights if it's an unhandled button
            // currentActiveContext = ActiveContext.None;
            // UpdateButtonHighlights();
            return;
        }

        Debug.Log($"NavBar: Navigating to external scene: {sceneNameIfExternal} for context {targetContext}");
        currentActiveContext = targetContext; // Set context for potential pre-load highlight
        UpdateButtonHighlights(); // Update highlights *before* scene load for immediate feedback

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneAsyncWithFade(sceneNameIfExternal);
        }
        else
        {
            Debug.LogWarning($"NavBarManager: SceneFader.Instance not found. Loading scene '{sceneNameIfExternal}' directly.");
            SceneManager.LoadScene(sceneNameIfExternal);
        }
    }


    public void UpdateButtonHighlights()
    {
        // Debug.Log($"NavBar: Updating highlights. Current context: {currentActiveContext}");
        HighlightButton(wishlistButton, currentActiveContext == ActiveContext.Wishlist);
        HighlightButton(appliedButton, currentActiveContext == ActiveContext.Applied);
        HighlightButton(homeButton, currentActiveContext == ActiveContext.Home);
        HighlightButton(searchButton, currentActiveContext == ActiveContext.Search);
        HighlightButton(profileButton, currentActiveContext == ActiveContext.Profile);
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
            buttonText.color = isActive ? activeTextColor : inactiveTextColor;
        }

        // Option 3: Make the active button non-interactable (common UX)
        button.interactable = !isActive;
    }
}