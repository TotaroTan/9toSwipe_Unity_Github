using UnityEngine;
using UnityEngine.UI;    // Still needed for Button component
using TMPro;             // Required for TextMeshProUGUI

public class MonetizationCanvasSwitcher : MonoBehaviour
{
    [Header("Monetization Page Canvases")]
    public GameObject userMonetizationCanvasGO;
    public GameObject businessMonetizationCanvasGO;

    // NEW: Reference to the Wishlist Canvas
    [Header("Wishlist Page Canvas")]
    public GameObject wishlistCanvasGO;

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

    // NEW: References to "Continue" Buttons
    [Header("Continue Buttons")]
    public Button userPage_ContinueBTN;       // Assign "...UserContentPanel/Continue button"
    public Button businessPage_ContinueBTN;   // Assign "...User content/Continue button" (under Business)

    [Header("Toggle Text Colors")]
    public Color activeColor = new Color32(1, 181, 129, 255);
    public Color inactiveColor = new Color32(123, 123, 131, 255);

    // Enum to track which monetization page was last active, or if Wishlist is active
    private enum ActivePageContext { UserMonetization, BusinessMonetization, Wishlist }
    private ActivePageContext currentPageContext;


    void Start()
    {
        // --- Essential Setup Checks ---
        if (userMonetizationCanvasGO == null) { Debug.LogError("Switcher: 'User Monetization Canvas GO' not assigned!"); enabled = false; return; }
        if (businessMonetizationCanvasGO == null) { Debug.LogError("Switcher: 'Business Monetization Canvas GO' not assigned!"); enabled = false; return; }
        if (wishlistCanvasGO == null) { Debug.LogError("Switcher: 'Wishlist Canvas GO' not assigned!"); enabled = false; return; } // NEW CHECK

        // --- User Page Button Listeners ---
        if (userPage_UserToggleBTN != null) userPage_UserToggleBTN.onClick.AddListener(ShowUserMonetizationPage);
        else Debug.LogError("Switcher: 'User Page User Toggle BTN' not assigned!");

        if (userPage_BusinessToggleBTN != null) userPage_BusinessToggleBTN.onClick.AddListener(ShowBusinessMonetizationPage);
        else Debug.LogError("Switcher: 'User Page Business Toggle BTN' not assigned!");

        // NEW: Listener for User Page Continue Button
        if (userPage_ContinueBTN != null) userPage_ContinueBTN.onClick.AddListener(ShowWishlistPage);
        else Debug.LogError("Switcher: 'User Page Continue BTN' not assigned!");


        // --- Business Page Button Listeners ---
        if (businessPage_UserToggleBTN != null) businessPage_UserToggleBTN.onClick.AddListener(ShowUserMonetizationPage);
        else Debug.LogError("Switcher: 'Business Page User Toggle BTN' not assigned!");

        if (businessPage_BusinessToggleBTN != null) businessPage_BusinessToggleBTN.onClick.AddListener(ShowBusinessMonetizationPage);
        else Debug.LogError("Switcher: 'Business Page Business Toggle BTN' not assigned!");

        // NEW: Listener for Business Page Continue Button
        if (businessPage_ContinueBTN != null) businessPage_ContinueBTN.onClick.AddListener(ShowWishlistPage);
        else Debug.LogError("Switcher: 'Business Page Continue BTN' not assigned!");

        // --- Initial State ---
        ShowUserMonetizationPage(); // Start with the User Monetization page visible
    }

    public void ShowUserMonetizationPage()
    {
        userMonetizationCanvasGO.SetActive(true);
        businessMonetizationCanvasGO.SetActive(false);
        wishlistCanvasGO.SetActive(false); // Ensure Wishlist is hidden

        currentPageContext = ActivePageContext.UserMonetization;
        UpdateAllToggleTextColors();

        Debug.Log("Switched to User Monetization Page.");
    }

    public void ShowBusinessMonetizationPage()
    {
        userMonetizationCanvasGO.SetActive(false);
        businessMonetizationCanvasGO.SetActive(true);
        wishlistCanvasGO.SetActive(false); // Ensure Wishlist is hidden

        currentPageContext = ActivePageContext.BusinessMonetization;
        UpdateAllToggleTextColors();

        Debug.Log("Switched to Business Monetization Page.");
    }

    // NEW: Method to show the Wishlist Page
    public void ShowWishlistPage()
    {
        userMonetizationCanvasGO.SetActive(false);    // Hide User Monetization
        businessMonetizationCanvasGO.SetActive(false); // Hide Business Monetization
        wishlistCanvasGO.SetActive(true);             // Show Wishlist

        currentPageContext = ActivePageContext.Wishlist;
        // When Wishlist is shown, the header toggles on the (now hidden) monetization pages
        // might not need to change, or you can set them to a default state.
        // The UpdateAllToggleTextColors will handle their appearance based on currentPageContext.
        UpdateAllToggleTextColors();

        Debug.Log("Switched to Wishlist Page.");
    }

    private void UpdateAllToggleTextColors()
    {
        bool isUserMonetizationActive = (currentPageContext == ActivePageContext.UserMonetization);
        bool isBusinessMonetizationActive = (currentPageContext == ActivePageContext.BusinessMonetization);

        // If on Wishlist page, determine how the (hidden) toggles should look.
        // For example, make "User" appear selected by default on both hidden headers.
        if (currentPageContext == ActivePageContext.Wishlist)
        {
            isUserMonetizationActive = true; // Or false, or based on a "last active" state
            isBusinessMonetizationActive = false;
        }

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