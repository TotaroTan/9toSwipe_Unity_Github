using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WishlistPageController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform wishlistContentArea;
    [SerializeField] private GameObject wishlistJobCardPrefab;
    [SerializeField] private TextMeshProUGUI jobCountText;

    [Header("Navigation Buttons")]
    [SerializeField] private Button homeNavButton;
    [SerializeField] private Button searchNavButton;
    [SerializeField] private Button starredNavButton; // This scene's button
    [SerializeField] private Button appliedNavButton;
    [SerializeField] private Button profileNavButton;

    private List<GameObject> _instantiatedCards = new List<GameObject>();

    void Start()
    {
        if (wishlistJobCardPrefab == null) Debug.LogError("[WishlistPageController] WishlistJobCardPrefab not assigned!");
        if (wishlistContentArea == null) Debug.LogError("[WishlistPageController] WishlistContentArea not assigned!");
        if (jobCountText == null) Debug.LogError("[WishlistPageController] JobCountText not assigned!");

        SetupNavigationButtonListeners();
        RefreshWishlistDisplay();
    }

    void OnEnable()
    {
        if (WishlistManager.Instance != null)
        {
            RefreshWishlistDisplay();
        }
    }

    void SetupNavigationButtonListeners()
    {
        if (homeNavButton) homeNavButton.onClick.AddListener(() => SceneManager.LoadScene("Home"));
        if (searchNavButton) searchNavButton.onClick.AddListener(() => SceneManager.LoadScene("SearchScene"));
        if (starredNavButton) starredNavButton.interactable = false; // Current scene
        if (appliedNavButton) appliedNavButton.onClick.AddListener(() => SceneManager.LoadScene("AppliedJobsScene")); // Or your "favorite.html" equivalent scene name
        if (profileNavButton) profileNavButton.onClick.AddListener(() => SceneManager.LoadScene("UserScene"));
    }

    public void RefreshWishlistDisplay()
    {
        if (WishlistManager.Instance == null)
        {
            Debug.LogError("[WishlistPageController] WishlistManager.Instance is null. Cannot display wishlist.");
            if(jobCountText != null) jobCountText.text = "Error";
            return;
        }

        foreach (GameObject card in _instantiatedCards)
        {
            if (card != null) Destroy(card);
        }
        _instantiatedCards.Clear();

        List<WishlistedJobData> wishlistedJobs = WishlistManager.Instance.GetWishlistedJobsData();
        Debug.Log($"[WishlistPageController] Refreshing. Found {wishlistedJobs.Count} wishlisted jobs.");

        if (jobCountText != null)
        {
            int count = wishlistedJobs.Count;
            jobCountText.text = $"{count} Job{(count == 1 ? "" : "s")} Wishlisted"; // Modified text
        }

        if (wishlistedJobs.Count == 0)
        {
            if (wishlistContentArea != null)
            {
                GameObject noJobsTextGO = new GameObject("NoWishlistedJobsText");
                noJobsTextGO.transform.SetParent(wishlistContentArea, false);
                RectTransform rt = noJobsTextGO.AddComponent<RectTransform>();
                TextMeshProUGUI tmp = noJobsTextGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "Your wishlist is empty.";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 20;
                tmp.color = Color.gray;
                rt.sizeDelta = new Vector2(wishlistContentArea.rect.width - 40, 100);
                _instantiatedCards.Add(noJobsTextGO);
            }
            return;
        }

        if (wishlistJobCardPrefab == null || wishlistContentArea == null) return;

        foreach (WishlistedJobData wishlistedJob in wishlistedJobs)
        {
            if (wishlistedJob == null) continue;

            GameObject cardInstance = Instantiate(wishlistJobCardPrefab, wishlistContentArea);
            WishlistJobCardController cardController = cardInstance.GetComponent<WishlistJobCardController>();

            if (cardController != null)
            {
                cardController.Setup(wishlistedJob /*, this */); // Pass 'this' if card needs to call back to page controller
                _instantiatedCards.Add(cardInstance);
            }
            else
            {
                Debug.LogError("[WishlistPageController] WishlistJobCardPrefab is missing WishlistJobCardController script!");
                Destroy(cardInstance);
            }
        }
    }
}