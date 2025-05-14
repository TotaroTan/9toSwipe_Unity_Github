using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System; // <<<--- ADDED THIS LINE

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
            DisplayJobs(new List<JobData>()); // Ensure empty state is handled
            Debug.LogWarning("No jobs loaded initially or allJobs list is empty after LoadJobData.");
        }


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
            allJobs = new List<JobData>(); // Ensure allJobs is initialized even on failure
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
        locationDropdown.RefreshShownValue(); // Ensure the first item is displayed
    }


    void DisplayJobs(List<JobData> jobsToDisplay)
    {
        if (jobListingsContentArea == null || jobCardPrefab == null)
        {
            // Errors already logged in Start(), but good to have a guard here.
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
            instantiatedJobCards.Add(noJobsTextGO); // Add to list so it gets cleared next time
            return;
        }

        foreach (JobData job in jobsToDisplay)
        {
            if (job == null)
            {
                Debug.LogWarning("Encountered a null job entry while displaying jobs.");
                continue; // Skip this null job
            }
            GameObject cardInstance = Instantiate(jobCardPrefab, jobListingsContentArea);

            // Populate cardInstance (Ensure your JobCardPrefab has these child TextMeshPro objects)
            // ADJUST THESE Find paths if your JobCardPrefab hierarchy is different.
            TMP_Text companyNameText = cardInstance.transform.Find("JobDetailsContainer/CompanyNameText")?.GetComponent<TMP_Text>();
            TMP_Text jobTitleText = cardInstance.transform.Find("JobDetailsContainer/JobTitleText")?.GetComponent<TMP_Text>();
            TMP_Text locationText = cardInstance.transform.Find("JobDetailsContainer/LocationText")?.GetComponent<TMP_Text>();
            // Add more for salary, deadline etc. if they are on your card prefab
            // TMP_Text salaryText = cardInstance.transform.Find("JobDetailsContainer/SalaryText")?.GetComponent<TMP_Text>();
            // TMP_Text deadlineText = cardInstance.transform.Find("JobDetailsContainer/DeadlineText")?.GetComponent<TMP_Text>();


            if (companyNameText) companyNameText.text = job.company ?? "N/A";
            else Debug.LogWarning($"CompanyNameText not found on JobCardPrefab for job: {job.company}");

            if (jobTitleText) jobTitleText.text = job.title ?? "N/A";
            else Debug.LogWarning($"JobTitleText not found on JobCardPrefab for job: {job.company}");

            if (locationText) locationText.text = job.location ?? "N/A";
            else Debug.LogWarning($"LocationText not found on JobCardPrefab for job: {job.company}");

            // if (salaryText) salaryText.text = $"Salary: {job.salary ?? "N/A"}";
            // if (deadlineText) deadlineText.text = $"Deadline: {job.deadline ?? "N/A"}";

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
            if (isActive) // If just opened
            {
                if (searchKeywordInput) searchKeywordInput.text = "";
                if (locationDropdown) locationDropdown.value = 0; // Reset to "All Locations"
                ClearSuggestions();
                searchKeywordInput.Select(); // Focus the input field
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
            ToggleSearchOverlay(); // Close overlay after search
        }
    }

    void OnSearchKeywordChanged(string newText)
    {
        ClearSuggestions();
        if (string.IsNullOrWhiteSpace(newText) || newText.Length < 2) // Min 2 chars for suggestions
        {
            return;
        }

        string searchTextLower = newText.ToLower();
        var suggestionsQuery = allJobs
            .Where(job => (job.company != null && job.company.ToLower().Contains(searchTextLower)) ||
                          (job.title != null && job.title.ToLower().Contains(searchTextLower)))
            .SelectMany(job => new[] { job.company, job.title }) // Get both company and title
            .Where(s => s != null && s.ToLower().Contains(searchTextLower)) // Filter again to be sure and handle nulls
            .Distinct(StringComparer.OrdinalIgnoreCase) // Make suggestions case-insensitive distinct
            .OrderBy(s => s)
            .Take(5); // Show top 5 suggestions

        DisplaySuggestions(suggestionsQuery.ToList());
    }

    void DisplaySuggestions(List<string> suggestions)
    {
        if (suggestionsContainer == null || suggestionButtonPrefab == null) return;

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
                    // Optionally, perform search immediately after clicking suggestion:
                    // OnPerformSearch();
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