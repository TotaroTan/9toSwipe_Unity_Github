// MonetizationCanvasSwitcher.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using UnityEngine.SceneManagement; // Only needed if you use the SceneManager.LoadScene fallback and want to shorten it

public class MonetizationCanvasSwitcher : MonoBehaviour
{
    [Header("Monetization Page Canvases")]
    public GameObject userMonetizationCanvasGO;
    public GameObject businessMonetizationCanvasGO;

    [Header("Scene Names")]
    public string wishlistSceneName = "ApplyAndWishlist"; // Ensure this matches your actual scene name

    [Header("User Page Header Elements")]
    public Button userPage_UserToggleBTN;
    public TextMeshProUGUI userPage_UserTextDisplay;
    public Button userPage_BusinessToggleBTN;
    public TextMeshProUGUI userPage_BusinessTextDisplay;

    [Header("Business Page Header Elements")]
    public Button businessPage_UserToggleBTN;
    public TextMeshProUGUI businessPage_UserTextDisplay;
    public Button businessPage_BusinessToggleBTN;
    public TextMeshProUGUI businessPage_BusinessTextDisplay;

    [Header("Continue Buttons")]
    public Button userPage_ContinueBTN;
    public Button businessPage_ContinueBTN;

    [Header("Toggle Text Colors")]
    public Color activeColor = new Color32(1, 181, 129, 255);
    public Color inactiveColor = new Color32(123, 123, 131, 255);

    private enum ActivePageContext { UserMonetization, BusinessMonetization }
    private ActivePageContext currentPageContext;


    void Start()
    {
        // --- Essential Setup Checks ---
        if (userMonetizationCanvasGO == null) { Debug.LogError("Switcher: 'User Monetization Canvas GO' not assigned!"); enabled = false; return; }
        if (businessMonetizationCanvasGO == null) { Debug.LogError("Switcher: 'Business Monetization Canvas GO' not assigned!"); enabled = false; return; }
        if (string.IsNullOrEmpty(wishlistSceneName)) { Debug.LogError("Switcher: 'Wishlist Scene Name' not assigned or empty!"); enabled = false; return; }
        
        // Check for SceneFader instance (it should be persistent from an earlier scene)
        if (SceneFader.Instance == null)
        {
            Debug.LogWarning("Switcher: SceneFader.Instance not found! Scene transitions will not fade. Ensure SceneFader is in an initial scene and persists.");
            // We don't disable the script entirely, as local canvas switching might still work,
            // but scene transitions via the buttons will fall back to direct loading if SceneFader is missing.
        }

        // --- User Page Button Listeners ---
        if (userPage_UserToggleBTN != null) userPage_UserToggleBTN.onClick.AddListener(ShowUserMonetizationPage);
        else Debug.LogError("Switcher: 'User Page User Toggle BTN' not assigned!");

        if (userPage_BusinessToggleBTN != null) userPage_BusinessToggleBTN.onClick.AddListener(ShowBusinessMonetizationPage);
        else Debug.LogError("Switcher: 'User Page Business Toggle BTN' not assigned!");

        if (userPage_ContinueBTN != null) userPage_ContinueBTN.onClick.AddListener(TransitionToWishlistScene);
        else Debug.LogError("Switcher: 'User Page Continue BTN' not assigned!");


        // --- Business Page Button Listeners ---
        if (businessPage_UserToggleBTN != null) businessPage_UserToggleBTN.onClick.AddListener(ShowUserMonetizationPage);
        else Debug.LogError("Switcher: 'Business Page User Toggle BTN' not assigned!");

        if (businessPage_BusinessToggleBTN != null) businessPage_BusinessToggleBTN.onClick.AddListener(ShowBusinessMonetizationPage);
        else Debug.LogError("Switcher: 'Business Page Business Toggle BTN' not assigned!");

        if (businessPage_ContinueBTN != null) businessPage_ContinueBTN.onClick.AddListener(TransitionToWishlistScene);
        else Debug.LogError("Switcher: 'Business Page Continue BTN' not assigned!");

        // --- Initial State ---
        ShowUserMonetizationPage(); // Start with the User Monetization page visible
    }

    public void ShowUserMonetizationPage()
    {
        if (userMonetizationCanvasGO != null) userMonetizationCanvasGO.SetActive(true);
        if (businessMonetizationCanvasGO != null) businessMonetizationCanvasGO.SetActive(false);
        // No direct interaction with wishlist canvas here as it's in another scene

        currentPageContext = ActivePageContext.UserMonetization;
        UpdateAllToggleTextColors();

        Debug.Log("Switched to User Monetization Page.");
    }

    public void ShowBusinessMonetizationPage()
    {
        if (userMonetizationCanvasGO != null) userMonetizationCanvasGO.SetActive(false);
        if (businessMonetizationCanvasGO != null) businessMonetizationCanvasGO.SetActive(true);
        // No direct interaction with wishlist canvas here

        currentPageContext = ActivePageContext.BusinessMonetization;
        UpdateAllToggleTextColors();

        Debug.Log("Switched to Business Monetization Page.");
    }

    public void TransitionToWishlistScene()
    {
        if (string.IsNullOrEmpty(wishlistSceneName))
        {
            Debug.LogError("MonetizationCanvasSwitcher: Wishlist Scene Name is not set in the Inspector!");
            return;
        }

        // Optionally hide current canvases before starting the fade.
        // This can sometimes prevent a visual flicker if the fade-out isn't instant.
        // if (userMonetizationCanvasGO != null) userMonetizationCanvasGO.SetActive(false);
        // if (businessMonetizationCanvasGO != null) businessMonetizationCanvasGO.SetActive(false);

        Debug.Log($"Requesting async load and fade to Wishlist Scene: {wishlistSceneName}");
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneAsyncWithFade(wishlistSceneName); // <<< CORRECTED METHOD NAME
        }
        else
        {
            Debug.LogWarning("SceneFader instance not found. Cannot fade to scene. Loading directly.");
            // Fallback to direct load if SceneFader isn't available
            UnityEngine.SceneManagement.SceneManager.LoadScene(wishlistSceneName);
        }
    }

    private void UpdateAllToggleTextColors()
    {
        bool isUserMonetizationActive = (currentPageContext == ActivePageContext.UserMonetization);
        bool isBusinessMonetizationActive = (currentPageContext == ActivePageContext.BusinessMonetization);

        // Update colors for the toggles within the "User Monetization Canvas" Header
        if (userPage_UserTextDisplay != null)
            userPage_UserTextDisplay.color = isUserMonetizationActive ? activeColor : inactiveColor;
        if (userPage_BusinessTextDisplay != null)
            userPage_BusinessTextDisplay.color = isBusinessMonetizationActive ? activeColor : inactiveColor;

        // Update colors for the toggles within the "Business Monetization Canvas" Header
        if (businessPage_UserTextDisplay != null)
            businessPage_UserTextDisplay.color = isUserMonetizationActive ? activeColor : inactiveColor;
        if (businessPage_BusinessTextDisplay != null)
            businessPage_BusinessTextDisplay.color = isBusinessMonetizationActive ? activeColor : inactiveColor;
    }
}