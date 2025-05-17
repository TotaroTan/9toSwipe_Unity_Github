using UnityEngine;
using UnityEngine.UI; // Required for Button, ScrollRect etc.
using TMPro;          // Required for TextMeshProUGUI
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Required for scene navigation

public class AppliedJobsPageController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform appliedJobsContentArea; // The 'Content' child of your ScrollView
    [SerializeField] private GameObject appliedJobCardPrefab;     // Prefab for individual applied job cards
    [SerializeField] private TextMeshProUGUI jobCountText;       // Text to display "X Jobs"

    [Header("Navigation Buttons")] // Assign these in the Unity Inspector
    [SerializeField] private Button homeNavButton;
    [SerializeField] private Button searchNavButton;
    [SerializeField] private Button starredNavButton; // Corresponds to "Monetization.html" equivalent
    [SerializeField] private Button appliedNavButton; // Corresponds to "favorite.html" (this scene), can be disabled
    [SerializeField] private Button profileNavButton;

    private List<GameObject> _instantiatedCards = new List<GameObject>();

    void Start()
    {
        // --- Initial Null Checks for Essential References ---
        if (appliedJobCardPrefab == null)
            Debug.LogError("[AppliedJobsPageController] AppliedJobCardPrefab is not assigned in the Inspector!");
        if (appliedJobsContentArea == null)
            Debug.LogError("[AppliedJobsPageController] AppliedJobsContentArea (ScrollView's Content) is not assigned in the Inspector!");
        if (jobCountText == null)
            Debug.LogError("[AppliedJobsPageController] JobCountText is not assigned in the Inspector!");
        // --- End Null Checks ---

        SetupNavigationButtonListeners();
        RefreshAppliedJobsDisplay(); // Initial display of applied jobs when the scene starts
    }

    void OnEnable()
    {
        // This method is called every time the GameObject becomes active.
        // Useful for refreshing the list if the user navigates away and then back to this scene.
        if (AppliedJobsManager.Instance != null) // Ensure the manager is ready
        {
            RefreshAppliedJobsDisplay();
        }
        else
        {
            // This might happen if this scene loads before AppliedJobsManager is initialized
            // Or if AppliedJobsManager was destroyed.
            Debug.LogWarning("[AppliedJobsPageController] OnEnable: AppliedJobsManager.Instance is null. Waiting for Start or check manager persistence.");
        }
    }

    void SetupNavigationButtonListeners()
    {
        // Ensure your scene names ("Home", "SearchScene", etc.) are correct and added to Build Settings.
        if (homeNavButton != null) homeNavButton.onClick.AddListener(() => SceneManager.LoadScene("Home")); // Replace "Home" with your actual Home scene name
        else Debug.LogWarning("[AppliedJobsPageController] HomeNavButton not assigned.");

        if (searchNavButton != null) searchNavButton.onClick.AddListener(() => SceneManager.LoadScene("SearchScene")); // Replace "SearchScene" if different
        else Debug.LogWarning("[AppliedJobsPageController] SearchNavButton not assigned.");

        if (starredNavButton != null) starredNavButton.onClick.AddListener(() => SceneManager.LoadScene("MonetizationScene")); // Replace "MonetizationScene" if different
        else Debug.LogWarning("[AppliedJobsPageController] StarredNavButton not assigned.");

        if (appliedNavButton != null)
        {
            appliedNavButton.interactable = false; // Visually indicate it's the current page by disabling it
        }
        else Debug.LogWarning("[AppliedJobsPageController] AppliedNavButton not assigned.");


        if (profileNavButton != null) profileNavButton.onClick.AddListener(() => SceneManager.LoadScene("UserScene")); // Replace "UserScene" if different
        else Debug.LogWarning("[AppliedJobsPageController] ProfileNavButton not assigned.");
    }

    /// <summary>
    /// Clears the current list of applied jobs and re-populates it by fetching
    /// the latest data from the AppliedJobsManager.
    /// </summary>
    public void RefreshAppliedJobsDisplay()
    {
        if (AppliedJobsManager.Instance == null)
        {
            Debug.LogError("[AppliedJobsPageController] AppliedJobsManager.Instance is null. Cannot display applied jobs. Ensure it's initialized and persistent.");
            if (jobCountText != null) jobCountText.text = "Error"; // Simplified error message
            return;
        }

        // Clear previously instantiated cards from the UI
        foreach (GameObject card in _instantiatedCards)
        {
            if (card != null) Destroy(card);
        }
        _instantiatedCards.Clear();

        // Get the current list of applied JobData objects
        List<JobData> appliedJobsData = AppliedJobsManager.Instance.GetAppliedJobsData();
        Debug.Log($"[AppliedJobsPageController] Refreshing display. Found {appliedJobsData.Count} applied jobs from manager.");


        // Update the job count text
        if (jobCountText != null)
        {
            int count = appliedJobsData.Count;
            jobCountText.text = $"{count} Job{(count == 1 ? "" : "s")}";
        }

        // If no jobs are applied, display a message
        if (appliedJobsData.Count == 0)
        {
            if (appliedJobsContentArea != null) // Ensure content area exists
            {
                GameObject noJobsTextGO = new GameObject("NoAppliedJobsText");
                noJobsTextGO.transform.SetParent(appliedJobsContentArea, false); // SetParent correctly
                RectTransform rt = noJobsTextGO.AddComponent<RectTransform>(); // Add RectTransform for UI positioning

                TextMeshProUGUI tmp = noJobsTextGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "You haven't applied to any jobs yet.";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 20; // Adjust size as needed
                tmp.color = Color.gray; // Adjust color as needed

                // Optional: Set size for the message text object within the layout
                rt.sizeDelta = new Vector2(appliedJobsContentArea.rect.width - 40, 100); // Example size

                _instantiatedCards.Add(noJobsTextGO); // Add to list so it's cleared on next refresh
            }
            return;
        }

        // Instantiate and set up a card for each applied job
        if (appliedJobCardPrefab == null || appliedJobsContentArea == null)
        {
             Debug.LogError("[AppliedJobsPageController] AppliedJobCardPrefab or AppliedJobsContentArea is null. Cannot instantiate cards.");
            return;
        }

        foreach (JobData job in appliedJobsData)
        {
            if (job == null)
            {
                Debug.LogWarning("[AppliedJobsPageController] Encountered a null JobData in appliedJobsData list. Skipping.");
                continue;
            }

            GameObject cardInstance = Instantiate(appliedJobCardPrefab, appliedJobsContentArea);
            AppliedJobCardController cardController = cardInstance.GetComponent<AppliedJobCardController>();

            if (cardController != null)
            {
                cardController.Setup(job, this); // Pass 'this' controller for callbacks (like remove)
                _instantiatedCards.Add(cardInstance);
            }
            else
            {
                Debug.LogError("[AppliedJobsPageController] AppliedJobCardPrefab is missing the AppliedJobCardController script! Destroying instance.");
                Destroy(cardInstance); // Clean up if prefab is misconfigured
            }
        }
    }
}