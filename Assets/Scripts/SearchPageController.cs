using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO; // For Path.GetInvalidFileNameChars
using System.Text.RegularExpressions; // For Regex (more advanced sanitization if needed)

public class SearchPageController : MonoBehaviour
{
    [Header("UI References - Main Page")]
    public Button openSearchOverlayButton;
    public TextMeshProUGUI jobCountText;
    public RectTransform jobListingsContentArea;
    public GameObject jobCardPrefab;

    [Header("UI References - Search Overlay")]
    public GameObject searchOverlayPanel;
    public Button closeSearchOverlayButton;
    public TMP_InputField searchKeywordInput;
    public TMP_Dropdown locationDropdown;
    public Button performSearchButton;
    public RectTransform suggestionsContainer;
    public GameObject suggestionButtonPrefab;

    private List<JobData> allJobs = new List<JobData>();
    private List<JobData> currentlyDisplayedJobs = new List<JobData>();
    private List<GameObject> instantiatedJobCards = new List<GameObject>();
    private List<GameObject> instantiatedSuggestionButtons = new List<GameObject>();

    private const string ALL_LOCATIONS_OPTION = "All Locations";
    private readonly List<string> predefinedLocations = new List<string> { "Ho Chi Minh City", "Hanoi" };

    // IMPORTANT: If you have a generic fallback logo INSIDE Assets/Resources/Logos/square-logo/
    // (e.g., "default-logo.png"), set its name here (without extension).
    // If you don't have one, leave it empty or comment it out, and the logo will be hidden if not found.
    private const string FALLBACK_LOGO_FILENAME_INSIDE_SQUARE_LOGO_FOLDER = "default"; // e.g., "default-logo"


    void Start()
    {
        LoadJobData();
        PopulateLocationDropdown();

        if (allJobs.Count > 0)
        {
            DisplayJobs(allJobs);
        }
        else
        {
            DisplayJobs(new List<JobData>());
            Debug.LogWarning("No jobs loaded initially or allJobs list is empty after LoadJobData.");
        }

        // ... (rest of your Start method's UI assignments and checks remain the same) ...
        if (openSearchOverlayButton) openSearchOverlayButton.onClick.AddListener(ToggleSearchOverlay);
        else Debug.LogError("OpenSearchOverlayButton not assigned in SearchPageController Inspector.");

        if (searchOverlayPanel) searchOverlayPanel.SetActive(false);
        else Debug.LogError("SearchOverlayPanel not assigned in SearchPageController Inspector.");

        if (closeSearchOverlayButton) closeSearchOverlayButton.onClick.AddListener(ToggleSearchOverlay);
        else Debug.LogError("CloseSearchOverlayButton not assigned in SearchPageController Inspector.");

        if (performSearchButton) performSearchButton.onClick.AddListener(OnPerformSearch);
        else Debug.LogError("PerformSearchButton not assigned in SearchPageController Inspector.");

        if (searchKeywordInput) searchKeywordInput.onValueChanged.AddListener(OnSearchKeywordChanged);
        else Debug.LogError("SearchKeywordInput not assigned in SearchPageController Inspector.");

        if (suggestionButtonPrefab == null) Debug.LogError("SuggestionButtonPrefab not assigned in SearchPageController Inspector.");
        if (jobCardPrefab == null) Debug.LogError("JobCardPrefab not assigned in SearchPageController Inspector.");
        if (jobListingsContentArea == null) Debug.LogError("JobListingsContentArea not assigned in SearchPageController Inspector.");
        if (jobCountText == null) Debug.LogError("JobCountText not assigned in SearchPageController Inspector.");
        if (suggestionsContainer == null) Debug.LogError("SuggestionsContainer not assigned in SearchPageController Inspector.");
        if (locationDropdown == null) Debug.LogError("LocationDropdown not assigned in SearchPageController Inspector.");
    }

    void LoadJobData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("companies");
        if (jsonFile == null)
        {
            Debug.LogError("Cannot find 'companies.json' in Assets/Resources folder!");
            allJobs = new List<JobData>();
            return;
        }

        JobListContainer jobListContainer = null;
        try
        {
            jobListContainer = JsonUtility.FromJson<JobListContainer>(jsonFile.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing 'companies.json': {ex.Message}. Ensure JSON is valid.");
            allJobs = new List<JobData>();
            return;
        }

        if (jobListContainer != null && jobListContainer.jobs != null)
        {
            allJobs = new List<JobData>(jobListContainer.jobs);
            Debug.Log($"Loaded {allJobs.Count} jobs from companies.json.");
        }
        else
        {
            Debug.LogError("Failed to parse jobs from JSON (jobListContainer or jobListContainer.jobs is null) or JSON is empty/malformed.");
            allJobs = new List<JobData>();
        }
    }

    void PopulateLocationDropdown()
    {
        if (locationDropdown == null) return;
        locationDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData(ALL_LOCATIONS_OPTION));

        foreach (string loc in predefinedLocations)
        {
            options.Add(new TMP_Dropdown.OptionData(loc));
        }
        locationDropdown.AddOptions(options);
        locationDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Converts a company name to a suitable filename format.
    /// Example: "FPT Software" might become "fpt-software" or "fptsoftware".
    /// Adjust this logic to match how your logo PNGs are named.
    /// </summary>
    string SanitizeCompanyNameForLogo(string companyName)
    {
        if (string.IsNullOrEmpty(companyName))
            return string.Empty;

        string sanitized = companyName.ToLowerInvariant(); // Convert to lowercase

        // Replace common problematic characters or words if needed
        // sanitized = sanitized.Replace(" inc.", "").Replace(" ltd.", "").Replace(" co.", "");
        // sanitized = sanitized.Replace(".", ""); // Remove periods

        // Replace spaces and other non-alphanumeric (except hyphen) with a hyphen or nothing
        // This regex keeps letters, numbers, and hyphens, and converts sequences of other chars to a single hyphen.
        // You might want to just remove them instead of converting to hyphen.
        sanitized = Regex.Replace(sanitized, @"[^a-z0-9\-]+", "-");
        sanitized = Regex.Replace(sanitized, @"-+", "-"); // Replace multiple hyphens with a single one
        sanitized = sanitized.Trim('-'); // Remove leading/trailing hyphens

        // Based on your screenshot, many logos are single words or use hyphens e.g. "fpt-software"
        // This simple sanitization might be enough:
        // string simpleSanitized = companyName.ToLowerInvariant();
        // simpleSanitized = Regex.Replace(simpleSanitized, @"\s+", "-"); // Replace spaces with hyphens
        // simpleSanitized = new string(simpleSanitized.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        // return simpleSanitized;

        // The most crucial part is that this function's output must match your actual filenames.
        // For example, if company is "FPT Software" in JSON, and your logo is "fpt-software.png",
        // this function should return "fpt-software".
        // If company is "Gojek" and logo is "gojek.png", it should return "gojek".

        // A very simple approach if your filenames are just lowercase company names with spaces removed:
        // return companyName.ToLowerInvariant().Replace(" ", "");

        // Let's use a slightly more robust one that aims for the "fpt-software" style
        string name = companyName.ToLowerInvariant();
        // Remove common suffixes that might not be in filenames
        name = name.Replace(" inc.", "").Replace(" ltd.", "").Replace(" co.", "").Replace(" llc", "").Replace(" group", "");
        // Replace spaces and some special characters with hyphens
        name = Regex.Replace(name, @"[\s.&']+", "-");
        // Remove any remaining non-alphanumeric characters except hyphens
        name = Regex.Replace(name, @"[^a-z0-9\-]", "");
        // Trim trailing/leading hyphens
        name = name.Trim('-');
        // Prevent multiple hyphens
        name = Regex.Replace(name, @"-+", "-");

        return name;
    }


    void DisplayJobs(List<JobData> jobsToDisplay)
    {
        if (jobListingsContentArea == null || jobCardPrefab == null)
        {
            return;
        }

        foreach (GameObject card in instantiatedJobCards)
        {
            Destroy(card);
        }
        instantiatedJobCards.Clear();
        currentlyDisplayedJobs = jobsToDisplay;

        if (jobsToDisplay == null || jobsToDisplay.Count == 0)
        {
            if (jobCountText) jobCountText.text = "0 Jobs";
            GameObject noJobsTextGO = new GameObject("NoJobsText");
            noJobsTextGO.transform.SetParent(jobListingsContentArea, false);
            TextMeshProUGUI tmp = noJobsTextGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "No jobs found matching your criteria.";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 28;
            tmp.color = new Color(0.5f, 0.5f, 0.5f);
            instantiatedJobCards.Add(noJobsTextGO);
            return;
        }

        foreach (JobData job in jobsToDisplay)
        {
            if (job == null)
            {
                Debug.LogWarning("Encountered a null job entry while displaying jobs.");
                continue;
            }
            GameObject cardInstance = Instantiate(jobCardPrefab, jobListingsContentArea);

            // --- DYNAMIC LOGO LOADING ---
            Image companyLogoImage = cardInstance.transform.Find("CompanyLogoImage")?.GetComponent<Image>();
            if (companyLogoImage != null)
            {
                Sprite logoSprite = null;
                if (!string.IsNullOrEmpty(job.company))
                {
                    string sanitizedLogoName = SanitizeCompanyNameForLogo(job.company);
                    if (!string.IsNullOrEmpty(sanitizedLogoName))
                    {
                        string logoPath = $"Logos/square-logo/{sanitizedLogoName}";
                        logoSprite = Resources.Load<Sprite>(logoPath);

                        if (logoSprite == null)
                        {
                            Debug.LogWarning($"Specific logo not found for company '{job.company}' (tried path: '{logoPath}'). " +
                                             $"Ensure a file like '{sanitizedLogoName}.png' exists in 'Assets/Resources/Logos/square-logo/'.");
                        }
                    }
                }

                // Fallback if specific logo not found or company name was empty
                if (logoSprite == null && !string.IsNullOrEmpty(FALLBACK_LOGO_FILENAME_INSIDE_SQUARE_LOGO_FOLDER))
                {
                    string fallbackPath = $"Logos/square-logo/{FALLBACK_LOGO_FILENAME_INSIDE_SQUARE_LOGO_FOLDER}";
                    logoSprite = Resources.Load<Sprite>(fallbackPath);
                    if (logoSprite == null)
                    {
                        Debug.LogWarning($"Fallback logo '{fallbackPath}' also not found.");
                    }
                }

                if (logoSprite != null)
                {
                    companyLogoImage.sprite = logoSprite;
                    companyLogoImage.gameObject.SetActive(true);
                }
                else
                {
                    companyLogoImage.gameObject.SetActive(false); // Hide if no logo could be loaded
                    // Warning already logged if specific logo not found, and if fallback not found.
                }
            }
            else
            {
                Debug.LogWarning("CompanyLogoImage component not found on JobCardPrefab. Check prefab hierarchy and that its name is 'CompanyLogoImage'.");
            }
            // --- END DYNAMIC LOGO LOADING ---


            TMP_Text companyNameText = cardInstance.transform.Find("JobDetailsContainer/CompanyNameText")?.GetComponent<TMP_Text>();
            TMP_Text jobTitleText = cardInstance.transform.Find("JobDetailsContainer/JobTitleText")?.GetComponent<TMP_Text>();
            TMP_Text locationText = cardInstance.transform.Find("JobDetailsContainer/LocationText")?.GetComponent<TMP_Text>();
            TMP_Text salaryText = cardInstance.transform.Find("JobDetailsContainer/SalaryText")?.GetComponent<TMP_Text>();
            TMP_Text deadlineText = cardInstance.transform.Find("JobDetailsContainer/DeadlineText")?.GetComponent<TMP_Text>();

            if (companyNameText) companyNameText.text = !string.IsNullOrEmpty(job.company) ? job.company : "N/A";
            else Debug.LogWarning($"CompanyNameText not found on JobCardPrefab for job: {job.company}");

            if (jobTitleText) jobTitleText.text = !string.IsNullOrEmpty(job.title) ? job.title : "N/A";
            else Debug.LogWarning($"JobTitleText not found on JobCardPrefab for job: {job.company}");

            if (locationText) locationText.text = !string.IsNullOrEmpty(job.location) ? $"Location: {job.location}" : "Location: N/A";
            else Debug.LogWarning($"LocationText not found on JobCardPrefab for job: {job.company}");
            
            if (salaryText) salaryText.text = !string.IsNullOrEmpty(job.salary) ? $"Salary: {job.salary}" : "Salary: N/A";
            else Debug.LogWarning($"SalaryText not found on JobCardPrefab for job: {job.company}");

            if (deadlineText) deadlineText.text = !string.IsNullOrEmpty(job.deadline) ? $"Deadline: {job.deadline}" : "Deadline: N/A";
            else Debug.LogWarning($"DeadlineText not found on JobCardPrefab for job: {job.company}");

            instantiatedJobCards.Add(cardInstance);
        }
        if (jobCountText) jobCountText.text = $"{jobsToDisplay.Count} Jobs";
    }

    void ToggleSearchOverlay()
    {
        if (searchOverlayPanel)
        {
            bool isActive = !searchOverlayPanel.activeSelf;
            searchOverlayPanel.SetActive(isActive);
            if (isActive)
            {
                if (searchKeywordInput) searchKeywordInput.text = "";
                if (locationDropdown) locationDropdown.value = 0;
                ClearSuggestions();
                searchKeywordInput.Select();
                searchKeywordInput.ActivateInputField();
            }
        }
    }

    void OnPerformSearch()
    {
        string keyword = "";
        if (searchKeywordInput) keyword = searchKeywordInput.text.Trim().ToLower();

        string selectedLocation = "";
        if (locationDropdown != null && locationDropdown.options.Count > 0 && locationDropdown.value >= 0)
        {
            selectedLocation = locationDropdown.options[locationDropdown.value].text;
        }

        List<JobData> filteredJobs = new List<JobData>(allJobs);

        if (!string.IsNullOrEmpty(keyword))
        {
            filteredJobs = filteredJobs.Where(job =>
                (job.company != null && job.company.ToLower().Contains(keyword)) ||
                (job.title != null && job.title.ToLower().Contains(keyword))
            ).ToList();
        }

        if (!string.IsNullOrEmpty(selectedLocation) && selectedLocation != ALL_LOCATIONS_OPTION)
        {
            filteredJobs = filteredJobs.Where(job =>
                job.location != null && job.location.Equals(selectedLocation, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        DisplayJobs(filteredJobs);
        if (searchOverlayPanel && searchOverlayPanel.activeSelf)
        {
            ToggleSearchOverlay();
        }
    }

    void OnSearchKeywordChanged(string newText)
    {
        ClearSuggestions();
        if (string.IsNullOrWhiteSpace(newText) || newText.Length < 2)
        {
            return;
        }

        string searchTextLower = newText.ToLower();
        var suggestionsQuery = allJobs
            .Where(job => (job.company != null && job.company.ToLower().Contains(searchTextLower)) ||
                          (job.title != null && job.title.ToLower().Contains(searchTextLower)))
            .SelectMany(job => new[] { job.company, job.title })
            .Where(s => s != null && s.ToLower().Contains(searchTextLower))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .Take(5);

        DisplaySuggestions(suggestionsQuery.ToList());
    }

    void DisplaySuggestions(List<string> suggestions)
    {
        if (suggestionsContainer == null || suggestionButtonPrefab == null) return;
        ClearSuggestions();
        if (suggestions.Count == 0) return;

        foreach (string suggestionText in suggestions)
        {
            GameObject suggestionButtonGO = Instantiate(suggestionButtonPrefab, suggestionsContainer);
            TMP_Text buttonText = suggestionButtonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText) buttonText.text = suggestionText;
            else Debug.LogWarning("SuggestionButtonPrefab is missing a TextMeshProUGUI child for its text.");

            Button button = suggestionButtonGO.GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(() => {
                    if (searchKeywordInput) searchKeywordInput.text = suggestionText;
                    ClearSuggestions();
                });
            }
            else Debug.LogWarning("SuggestionButtonPrefab is missing a Button component.");
            instantiatedSuggestionButtons.Add(suggestionButtonGO);
        }
    }

    void ClearSuggestions()
    {
        if (suggestionsContainer == null) return;
        foreach (GameObject btn in instantiatedSuggestionButtons)
        {
            Destroy(btn);
        }
        instantiatedSuggestionButtons.Clear();
    }
}