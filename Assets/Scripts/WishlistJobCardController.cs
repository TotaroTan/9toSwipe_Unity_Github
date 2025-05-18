using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class WishlistJobCardController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image companyLogoImage;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI jobTitleText;
    [SerializeField] private TextMeshProUGUI statusText; // To display Closed, Pending, Ready

    // Reference to the page controller if you need to interact with it (e.g., if you add a remove button later)
    // private WishlistPageController _pageController;

    // Helper function to create a logo filename from company name
    private string SanitizeCompanyNameForLogo(string companyName)
    {
        if (string.IsNullOrEmpty(companyName)) return string.Empty;
        string name = companyName.ToLowerInvariant();
        name = name.Replace(" inc.", "").Replace(" ltd.", "").Replace(" co.", "").Replace(" llc", "").Replace(" group", "");
        name = Regex.Replace(name, @"[\s.&']+", "-");
        name = Regex.Replace(name, @"[^a-z0-9\-]", "");
        name = name.Trim('-');
        name = Regex.Replace(name, @"-+", "-");
        return name;
    }

    public void Setup(WishlistedJobData wishlistedJobData /*, WishlistPageController pageController */)
    {
        // _pageController = pageController;
        if (wishlistedJobData == null || wishlistedJobData.jobDetails == null)
        {
            Debug.LogError("[WishlistJobCardController] Setup called with null wishlistedJobData or jobDetails.");
            gameObject.SetActive(false); // Hide the card if data is bad
            return;
        }

        JobData jobData = wishlistedJobData.jobDetails;

        if (companyNameText != null) companyNameText.text = jobData.company ?? "N/A";
        if (jobTitleText != null) jobTitleText.text = jobData.title ?? "N/A";
        if (statusText != null)
        {
            statusText.text = $"Status: {wishlistedJobData.status ?? "Unknown"}";
            // Optional: Change color based on status
            switch (wishlistedJobData.status?.ToLower())
            {
                case "ready":
                    statusText.color = Color.green; // Example color
                    break;
                case "pending":
                    statusText.color = Color.yellow; // Example color
                    break;
                case "closed":
                    statusText.color = Color.red; // Example color
                    break;
                default:
                    statusText.color = Color.white;
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
                companyLogoImage.gameObject.SetActive(false);
                Debug.LogWarning($"Logo not found for wishlisted job company: '{jobData.company}' at path 'Logos/square-logo/{SanitizeCompanyNameForLogo(jobData.company)}'");
            }
        }
    }
}