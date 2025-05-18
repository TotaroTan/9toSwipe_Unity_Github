using UnityEngine;
using UnityEngine.UI;
using TMPro;
// REMOVE: using UnityEngine.SceneManagement; // SceneFader will handle this

public class MonetizationCanvasSwitcher : MonoBehaviour
{
    [Header("Monetization Page Canvases")]
    public GameObject userMonetizationCanvasGO;
    public GameObject businessMonetizationCanvasGO;

    [Header("Scene Names")]
    public string wishlistSceneName = "ApplyAndWishlist";

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
        if (userMonetizationCanvasGO == null) { Debug.LogError("Switcher: 'User Monetization Canvas GO' not assigned!"); enabled = false; return; }
        if (businessMonetizationCanvasGO == null) { Debug.LogError("Switcher: 'Business Monetization Canvas GO' not assigned!"); enabled = false; return; }
        if (string.IsNullOrEmpty(wishlistSceneName)) { Debug.LogError("Switcher: 'Wishlist Scene Name' not assigned or empty!"); enabled = false; return; }
        if (SceneFader.Instance == null) { Debug.LogError("Switcher: SceneFader.Instance not found! Make sure a SceneFader object exists in the scene."); enabled = false; return; }

        if (userPage_UserToggleBTN != null) userPage_UserToggleBTN.onClick.AddListener(ShowUserMonetizationPage);
        else Debug.LogError("Switcher: 'User Page User Toggle BTN' not assigned!");
        if (userPage_BusinessToggleBTN != null) userPage_BusinessToggleBTN.onClick.AddListener(ShowBusinessMonetizationPage);
        else Debug.LogError("Switcher: 'User Page Business Toggle BTN' not assigned!");
        if (userPage_ContinueBTN != null) userPage_ContinueBTN.onClick.AddListener(TransitionToWishlistScene);
        else Debug.LogError("Switcher: 'User Page Continue BTN' not assigned!");

        if (businessPage_UserToggleBTN != null) businessPage_UserToggleBTN.onClick.AddListener(ShowUserMonetizationPage);
        else Debug.LogError("Switcher: 'Business Page User Toggle BTN' not assigned!");
        if (businessPage_BusinessToggleBTN != null) businessPage_BusinessToggleBTN.onClick.AddListener(ShowBusinessMonetizationPage);
        else Debug.LogError("Switcher: 'Business Page Business Toggle BTN' not assigned!");
        if (businessPage_ContinueBTN != null) businessPage_ContinueBTN.onClick.AddListener(TransitionToWishlistScene);
        else Debug.LogError("Switcher: 'Business Page Continue BTN' not assigned!");

        ShowUserMonetizationPage();
    }

    public void ShowUserMonetizationPage()
    {
        userMonetizationCanvasGO.SetActive(true);
        businessMonetizationCanvasGO.SetActive(false);
        currentPageContext = ActivePageContext.UserMonetization;
        UpdateAllToggleTextColors();
        Debug.Log("Switched to User Monetization Page.");
    }

    public void ShowBusinessMonetizationPage()
    {
        userMonetizationCanvasGO.SetActive(false);
        businessMonetizationCanvasGO.SetActive(true);
        currentPageContext = ActivePageContext.BusinessMonetization;
        UpdateAllToggleTextColors();
        Debug.Log("Switched to Business Monetization Page.");
    }

    public void TransitionToWishlistScene()
    {
        // Optionally hide current canvases immediately
        // if (userMonetizationCanvasGO != null) userMonetizationCanvasGO.SetActive(false);
        // if (businessMonetizationCanvasGO != null) businessMonetizationCanvasGO.SetActive(false);

        Debug.Log($"Starting fade to Wishlist Scene: {wishlistSceneName}");
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene(wishlistSceneName);
        }
        else
        {
            Debug.LogError("SceneFader instance not found. Cannot fade to scene.");
            // Fallback to direct load (will show blue screen)
            // UnityEngine.SceneManagement.SceneManager.LoadScene(wishlistSceneName);
        }
    }

    private void UpdateAllToggleTextColors()
    {
        bool isUserMonetizationActive = (currentPageContext == ActivePageContext.UserMonetization);
        bool isBusinessMonetizationActive = (currentPageContext == ActivePageContext.BusinessMonetization);

        if (userPage_UserTextDisplay != null)
            userPage_UserTextDisplay.color = isUserMonetizationActive ? activeColor : inactiveColor;
        if (userPage_BusinessTextDisplay != null)
            userPage_BusinessTextDisplay.color = isBusinessMonetizationActive ? activeColor : inactiveColor;

        if (businessPage_UserTextDisplay != null)
            businessPage_UserTextDisplay.color = isUserMonetizationActive ? activeColor : inactiveColor;
        if (businessPage_BusinessTextDisplay != null)
            businessPage_BusinessTextDisplay.color = isBusinessMonetizationActive ? activeColor : inactiveColor;
    }
}