using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions; // For logo name sanitization

public class AppliedJobCardController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image companyLogoImage;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI jobTitleText;
    [SerializeField] private Button removeButton;

    private JobData _currentJobData;
    private AppliedJobsPageController _pageController;

    // Helper function to create a logo filename from company name
    // This should ideally match the logic in your HomeScene's JobCardController if it also loads logos
    private string SanitizeCompanyNameForLogo(string companyName)
    {
        if (string.IsNullOrEmpty(companyName)) return string.Empty;
        string name = companyName.ToLowerInvariant();
        name = name.Replace(" inc.", "").Replace(" ltd.", "").Replace(" co.", "").Replace(" llc", "").Replace(" group", "");
        name = Regex.Replace(name, @"[\s.&']+", "-"); // Replace spaces and some special chars with hyphens
        name = Regex.Replace(name, @"[^a-z0-9\-]", ""); // Remove any remaining non-alphanumeric except hyphens
        name = name.Trim('-'); // Trim trailing/leading hyphens
        name = Regex.Replace(name, @"-+", "-"); // Prevent multiple hyphens
        return name;
    }

    public void Setup(JobData jobData, AppliedJobsPageController pageController)
    {
        _currentJobData = jobData;
        _pageController = pageController;

        if (companyNameText != null) companyNameText.text = _currentJobData.company ?? "N/A";
        if (jobTitleText != null) jobTitleText.text = _currentJobData.title ?? "N/A";

        if (companyLogoImage != null)
        {
            Sprite logoSprite = null;
            if (!string.IsNullOrEmpty(_currentJobData.company))
            {
                string logoFileName = SanitizeCompanyNameForLogo(_currentJobData.company);
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
                Debug.LogWarning($"Logo not found for applied job company: '{_currentJobData.company}' at path 'Logos/square-logo/{SanitizeCompanyNameForLogo(_currentJobData.company)}'");
            }
        }

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners(); // Important to prevent multiple subscriptions
            removeButton.onClick.AddListener(OnRemoveClicked);
        }
    }

    private void OnRemoveClicked()
    {
        if (_currentJobData != null && AppliedJobsManager.Instance != null)
        {
            AppliedJobsManager.Instance.RemoveAppliedJob(_currentJobData);

            // Tell the page controller to refresh the list, which will inherently remove this card
            if (_pageController != null)
            {
                _pageController.RefreshAppliedJobsDisplay();
            }
            // No need to Destroy(gameObject) here, the PageController will handle it during refresh.
        }
    }
}
