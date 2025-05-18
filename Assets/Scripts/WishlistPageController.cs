using UnityEngine;
using UnityEngine.UI; // Required for Button, ScrollRect etc.
using TMPro;          // Required for TextMeshProUGUI
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Required for scene navigation

public class WishlistPageController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform wishlistContentArea;    // The 'Content' child of your ScrollView
    [SerializeField] private GameObject wishlistJobCardPrefab;  // Prefab for individual wishlisted job cards
    [SerializeField] private TextMeshProUGUI jobCountText;        // Text to display "X Jobs Wishlisted"

    [Header("Navigation Buttons")] // Assign these in the Unity Inspector
    [SerializeField] private Button homeNavButton;
    [SerializeField] private Button searchNavButton;
    [SerializeField] private Button starredNavButton; // This is this scene's button (Monetization.html equivalent)
    [SerializeField] private Button appliedNavButton; // Button to go to "Applied Jobs" scene
    [SerializeField] private Button profileNavButton;

    private List<GameObject> _instantiatedCards = new List<GameObject>();

    void Start()
    {
        // --- Initial Null Checks for Essential References ---
        if (wishlistJobCardPrefab == null)
            Debug.LogError("[WishlistPageController] WishlistJobCardPrefab is not assigned in the Inspector!");
        if (wishlistContentArea == null)
            Debug.LogError("[WishlistPageController] WishlistContentArea (ScrollView's Content) is not assigned in the Inspector!");
        if (jobCountText == null)
            Debug.LogError("[WishlistPageController] JobCountText is not assigned in the Inspector!");
        // --- End Null Checks ---

        SetupNavigationButtonListeners();
        RefreshWishlistDisplay(); // Initial display of wishlisted jobs when the scene starts
    }

    void OnEnable()
    {
        // This method is called every time the GameObject becomes active.
        // Useful for refreshing the list if the user navigates away and then back to this scene.
        if (WishlistManager.Instance != null) // Ensure the manager is ready
        {
            RefreshWishlistDisplay();
        }
        else
        {
            Debug.LogWarning("[WishlistPageController] OnEnable: WishlistManager.Instance is null. Waiting for Start or check manager persistence.");
        }
    }

    void SetupNavigationButtonListeners()
    {
        // Ensure your scene names ("Home", "SearchScene", etc.) are correct and added to Build Settings.
        if (homeNavButton != null) homeNavButton.onClick.AddListener(() => SceneManager.LoadScene("Home")); // Replace "Home" with your actual Home scene name
        else Debug.LogWarning("[WishlistPageController] HomeNavButton not assigned.");

        if (searchNavButton != null) searchNavButton.onClick.AddListener(() => SceneManager.LoadScene("SearchScene")); // Replace "SearchScene" if different
        else Debug.LogWarning("[WishlistPageController] SearchNavButton not assigned.");

        if (starredNavButton != null)
        {
            // This is the current scene's button, so disable it to indicate it's active
            starredNavButton.interactable = false;
        }
        else Debug.LogWarning("[WishlistPageController] StarredNavButton (this scene's button) not assigned.");

        if (appliedNavButton != null) appliedNavButton.onClick.AddListener(() => SceneManager.LoadScene("AppliedJobsScene")); // Replace "AppliedJobsScene" with your "favorite.html" equivalent scene name
        else Debug.LogWarning("[WishlistPageController] AppliedNavButton not assigned.");

        if (profileNavButton != null) profileNavButton.onClick.AddListener(() => SceneManager.LoadScene("UserScene")); // Replace "UserScene" if different
        else Debug.LogWarning("[WishlistPageController] ProfileNavButton not assigned.");
    }

    /// <summary>
    /// Clears the current list of wishlisted jobs from the UI and re-populates it
    /// by fetching the latest data from the WishlistManager.
    /// </summary>
    public void RefreshWishlistDisplay()
    {
        if (WishlistManager.Instance == null)
        {
            Debug.LogError("[WishlistPageController] WishlistManager.Instance is null. Cannot display wishlist. Ensure it's initialized and persistent.");
            if (jobCountText != null) jobCountText.text = "Error"; // Simplified error message
            return;
        }

        // Clear previously instantiated cards from the UI
        foreach (GameObject card in _instantiatedCards)
        {
            if (card != null) Destroy(card);
        }
        _instantiatedCards.Clear();

        // Get the current list of WishlistedJobData objects
        List<WishlistedJobData> wishlistedJobs = WishlistManager.Instance.GetWishlistedJobsData();
        Debug.Log($"[WishlistPageController] Refreshing display. Found {wishlistedJobs.Count} wishlisted jobs from manager.");

        // Update the job count text
        if (jobCountText != null)
        {
            int count = wishlistedJobs.Count;
            jobCountText.text = $"{count} Job{(count == 1 ? "" : "s")} Wishlisted";
        }

        // If no jobs are wishlisted, display a message
        if (wishlistedJobs.Count == 0)
        {
            if (wishlistContentArea != null) // Ensure content area exists
            {
                GameObject noJobsTextGO = new GameObject("NoWishlistedJobsText");
                noJobsTextGO.transform.SetParent(wishlistContentArea, false); // SetParent correctly
                RectTransform rt = noJobsTextGO.AddComponent<RectTransform>(); // Add RectTransform for UI positioning

                TextMeshProUGUI tmp = noJobsTextGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "Your wishlist is empty.";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 20; // Adjust size as needed
                tmp.color = Color.gray; // Adjust color as needed

                // Optional: Set size for the message text object within the layout
                rt.sizeDelta = new Vector2(wishlistContentArea.rect.width - 40, 100); // Example size

                _instantiatedCards.Add(noJobsTextGO); // Add to list so it's cleared on next refresh
            }
            return;
        }

        // Instantiate and set up a card for each wishlisted job
        if (wishlistJobCardPrefab == null || wishlistContentArea == null)
        {
             Debug.LogError("[WishlistPageController] WishlistJobCardPrefab or WishlistContentArea is null. Cannot instantiate cards.");
            return;
        }

        foreach (WishlistedJobData wishlistedJob in wishlistedJobs)
        {
            if (wishlistedJob == null || wishlistedJob.jobDetails == null)
            {
                Debug.LogWarning("[WishlistPageController] Encountered a null WishlistedJobData or its jobDetails are null. Skipping.");
                continue;
            }

            GameObject cardInstance = Instantiate(wishlistJobCardPrefab, wishlistContentArea);
            WishlistJobCardController cardController = cardInstance.GetComponent<WishlistJobCardController>();

            if (cardController != null)
            {
                cardController.Setup(wishlistedJob, this); // Pass 'this' controller for callbacks (like remove)
                _instantiatedCards.Add(cardInstance);
            }
            else
            {
                Debug.LogError("[WishlistPageController] WishlistJobCardPrefab is missing the WishlistJobCardController script! Destroying instance.");
                Destroy(cardInstance); // Clean up if prefab is misconfigured
            }
        }
    }
}