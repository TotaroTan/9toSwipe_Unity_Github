using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions; // For logo name sanitization

public class WishlistJobCardController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image companyLogoImage;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI jobTitleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button removeButton; // <<<--- ADD THIS LINE

    private WishlistedJobData _currentWishlistedJobData;
    private WishlistPageController _pageController; // To notify when an item is removed

    // Helper function to create a logo filename from company name
    // Ensure this matches any similar function in other card controllers if logo logic is shared
    private string SanitizeCompanyNameForLogo(string companyName)
    {
        if (string.IsNullOrEmpty(companyName)) return string.Empty;
        string name = companyName.ToLowerInvariant();
        // Basic sanitization, adjust to match your logo filenames
        name = name.Replace(" inc.", "").Replace(" ltd.", "").Replace(" co.", "").Replace(" llc", "").Replace(" group", "");
        name = Regex.Replace(name, @"[\s.&']+", "-"); 
        name = Regex.Replace(name, @"[^a-z0-9\-]", ""); 
        name = name.Trim('-'); 
        name = Regex.Replace(name, @"-+", "-"); 
        return name;
    }

    // 'pageController' is passed in so this card can tell the page to refresh
    public void Setup(WishlistedJobData wishlistedJobData, WishlistPageController pageController)
    {
        _currentWishlistedJobData = wishlistedJobData;
        _pageController = pageController;

        if (_currentWishlistedJobData == null || _currentWishlistedJobData.jobDetails == null)
        {
            Debug.LogError("[WishlistJobCardController] Setup called with null wishlistedJobData or jobDetails.");
            gameObject.SetActive(false); // Hide the card if data is bad
            return;
        }

        JobData jobData = _currentWishlistedJobData.jobDetails;

        if (companyNameText != null) companyNameText.text = jobData.company ?? "N/A";
        if (jobTitleText != null) jobTitleText.text = jobData.title ?? "N/A";
        
        if (statusText != null)
        {
            statusText.text = $"Status: {_currentWishlistedJobData.status ?? "Unknown"}";
            // Optional: Change color based on status
            switch (_currentWishlistedJobData.status?.ToLower())
            {
                case "ready":
                    statusText.color = Color.green;
                    break;
                case "pending":
                    statusText.color = Color.yellow;
                    break;
                case "closed":
                    statusText.color = Color.red;
                    break;
                default:
                    statusText.color = Color.white; // Or your default text color
                    break;
            }
        }

        if (companyLogoImage != null)
        {
            Sprite logoSprite = null;
            if (!string.IsNullOrEmpty(jobData.company))
            {
                string logoFileName = SanitizeCompanyNameForLogo(jobData.company);
                if (!string.IsNullOrEmpty(logoFileName))
                {
                    // Logos are expected in "Assets/Resources/Logos/square-logo/companyname.png"
                    string logoPath = $"Logos/square-logo/{logoFileName}";
                    logoSprite = Resources.Load<Sprite>(logoPath);
                }
            }

            if (logoSprite != null)
            {
                companyLogoImage.sprite = logoSprite;
                companyLogoImage.gameObject.SetActive(true);
            }
            else
            {
                companyLogoImage.gameObject.SetActive(false); // Or set a placeholder
                Debug.LogWarning($"Logo not found for wishlisted job company: '{jobData.company}' at path 'Logos/square-logo/{SanitizeCompanyNameForLogo(jobData.company)}'");
            }
        }

        // --- Setup Remove Button Listener ---
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners(); // Clear existing listeners
            removeButton.onClick.AddListener(OnRemoveClicked);
        }
        else
        {
            Debug.LogWarning("[WishlistJobCardController] RemoveButton is not assigned in the Inspector for this card.");
        }
    }

    private void OnRemoveClicked()
    {
        if (_currentWishlistedJobData == null || _currentWishlistedJobData.jobDetails == null)
        {
            Debug.LogError("[WishlistJobCardController] Cannot remove: current wishlisted job data is null.");
            return;
        }

        if (WishlistManager.Instance != null)
        {
            // Use the original JobData to remove from the WishlistManager
            WishlistManager.Instance.RemoveFromWishlist(_currentWishlistedJobData.jobDetails);
            Debug.Log($"[WishlistJobCardController] Removed '{_currentWishlistedJobData.jobDetails.title}' from wishlist via button.");

            // Notify the page controller to refresh its display
            if (_pageController != null)
            {
                _pageController.RefreshWishlistDisplay();
            }
            else
            {
                Debug.LogWarning("[WishlistJobCardController] PageController reference is null. Cannot call RefreshWishlistDisplay.");
                // Fallback: If no page controller, just destroy this card.
                // However, the page controller should ideally handle the list refresh.
                // Destroy(gameObject); 
            }
        }
        else
        {
            Debug.LogError("[WishlistJobCardController] WishlistManager.Instance is null. Cannot remove from wishlist.");
        }
    }
}