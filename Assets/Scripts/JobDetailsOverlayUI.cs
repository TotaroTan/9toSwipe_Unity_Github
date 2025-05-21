using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Needed for String comparison if used

// Attach this script to the root GameObject of your Job Details Overlay Panel
public class JobDetailsOverlayUI : MonoBehaviour
{
    [Header("UI References")]
    // Assign the TMP_Text components from your overlay panel here
    public TextMeshProUGUI companyNameText;
    public TextMeshProUGUI jobTitleText;
    public TextMeshProUGUI descriptionText;

    // Add references for other detail fields from your InfoGridPanel hint
    public TextMeshProUGUI locationValueText; // Renamed from locationText based on screenshot hint
    public TextMeshProUGUI salaryText;
    public TextMeshProUGUI experienceText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI deadlineText;

    // Assign the Button components from your overlay panel here
    public Button applyButton;
    public Button closeButton; // Your "X" or placeholder button

    // Store the job data currently being displayed
    private JobData currentJobData;

    // Colors for the Apply button state - Set these in the Inspector
    [Header("Apply Button Colors")]
    public Color applyButtonDefaultColor = Color.white;
    public Color applyButtonAppliedColor = Color.green;

    void Awake()
    {
        // Add listeners to the buttons
        // Check for null before adding listeners
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyButtonClicked);
            // Capture the default color from the button's image
            Image buttonImage = applyButton.GetComponent<Image>();
             if(buttonImage != null)
             {
                 applyButtonDefaultColor = buttonImage.color; // Use the color already set in the editor
             }
        }
        else Debug.LogError("ApplyButton not assigned in JobDetailsOverlayUI!");

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide); // Close the overlay when close button is clicked
        }
         else Debug.LogError("CloseButton not assigned in JobDetailsOverlayUI!");

        // Ensure the overlay is hidden initially (can also be disabled in Editor GameObject)
         gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // Clean up listeners when the object is destroyed
        if (applyButton != null) applyButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Displays the job details overlay and populates its fields with provided JobData.
    /// </summary>
    /// <param name="jobData">The JobData object containing information to display.</param>
    public void Show(JobData jobData)
    {
        currentJobData = jobData; // Store the data

        // Populate the UI text elements with data from the jobData object
        // Use null/empty checks for robustness
        if (companyNameText) companyNameText.text = !string.IsNullOrEmpty(jobData?.company) ? jobData.company : "Company N/A";
        if (jobTitleText) jobTitleText.text = !string.IsNullOrEmpty(jobData?.title) ? jobData.title : "Title N/A";
        // Assuming DescriptionText exists as a direct child based on your screenshot hierarchy hint
        if (descriptionText) descriptionText.text = !string.IsNullOrEmpty(jobData?.description) ? jobData.description : "Description N/A";

        // Populate other fields based on your InfoGridPanel hint and JSON structure
        // Add null checks for jobData and the text component
        if (locationValueText) locationValueText.text = !string.IsNullOrEmpty(jobData?.location) ? jobData.location : "N/A";
        if (salaryText) salaryText.text = !string.IsNullOrEmpty(jobData?.salary) ? jobData.salary : "N/A";
        if (experienceText) experienceText.text = !string.IsNullOrEmpty(jobData?.experience) ? jobData.experience : "N/A";
        if (typeText) typeText.text = !string.IsNullOrEmpty(jobData?.type) ? jobData.type : "N/A";
        if (deadlineText) deadlineText.text = !string.IsNullOrEmpty(jobData?.deadline) ? jobData.deadline : "N/A";


        // Reset the apply button state (color and text) every time the overlay is shown
        if (applyButton != null)
        {
             Image buttonImage = applyButton.GetComponent<Image>();
             if(buttonImage != null) buttonImage.color = applyButtonDefaultColor; // Reset to default color

             TMP_Text buttonText = applyButton.GetComponentInChildren<TMP_Text>();
             if(buttonText != null) buttonText.text = "Apply"; // Reset button text to default

             applyButton.interactable = true; // Ensure the button is interactive when shown
        }

        // Activate the overlay panel's root GameObject
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the job details overlay.
    /// </summary>
    public void Hide()
    {
        // Deactivate the overlay panel's root GameObject
        gameObject.SetActive(false);
        currentJobData = null; // Clear the stored data
    }

    /// <summary>
    /// Handler for the Apply button click event.
    /// </summary>
    void OnApplyButtonClicked()
    {
        // Optional: Log the click event with details from the currently displayed job
        Debug.Log($"Apply button clicked for: {currentJobData?.title} at {currentJobData?.company}");

        // Change the Apply button color to indicate it's been clicked/applied
         if (applyButton != null)
        {
             Image buttonImage = applyButton.GetComponent<Image>();
             if(buttonImage != null) buttonImage.color = applyButtonAppliedColor; // Change to applied color

             // Optional: Change button text as well
             TMP_Text buttonText = applyButton.GetComponentInChildren<TMP_Text>();
             if(buttonText != null) buttonText.text = "Applied!";

            // Optional: Make the button non-interactable after clicking to prevent multiple clicks
            // applyButton.interactable = false;
        }

        // --- TODO: Add actual application logic here! ---
        // This is where you would handle the application itself, e.g.:
        // - Open the company's job application URL (if available in your JSON/data structure)
        // - Show a confirmation popup
        // - Send data to a server
        // -------------------------------------------

        // Example: Open the domain URL if available in your JobData (assuming domain is a property)
        // You'll need to add a 'public string domain;' field to your JobData class if it's not there.
        if (currentJobData != null && !string.IsNullOrEmpty(currentJobData.domain))
        {
            // Need to add "http://" or "https://" prefix for Application.OpenURL
            string url = currentJobData.domain;
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url; // Or https, depending on common practice for domains
            }
            // Check if the URL is valid before opening
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                 Application.OpenURL(url);
                 Debug.Log($"Opening URL: {url}");
            }
            else
            {
                 Debug.LogWarning($"Invalid or unsupported URL format for domain '{currentJobData.domain}'. Cannot open.");
            }
        }
        else
        {
             Debug.LogWarning("No valid domain URL available for application.");
        }
    }

    // You can add public methods here for other buttons/interactions on the overlay,
    // like a "Save/Wishlist" button if needed.
    // void OnWishlistButtonClicked()
    // {
    //     Debug.Log($"Wishlist button clicked for: {currentJobData?.title}");
    //     // Add wishlist logic here
    // }
}