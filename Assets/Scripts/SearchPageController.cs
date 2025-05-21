using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO; // For Path.GetInvalidFileNameChars (though we're using regex)
using System.Text.RegularExpressions; // For Regex

// The main class for the search page controller
public class SearchPageController : MonoBehaviour
{
    // --- UI References ---
    [Header("UI References - Main Page")]
    public Button openSearchOverlayButton; // Button to open the search overlay
    public TextMeshProUGUI jobCountText; // Text element to display the number of jobs
    // Assign the "Content" GameObject inside your Scroll View here.
    // This is where the job cards will be instantiated.
    // IMPORTANT: Ensure this GameObject has the correct UI components (Vertical Layout Group, Content Size Fitter)
    // configured as described previously to fix scrolling jiggling.
    public RectTransform jobListingsContentArea;
    public GameObject jobCardPrefab; // Assign your Job Card Prefab here

    [Header("UI References - Search Overlay")]
    public GameObject searchOverlayPanel; // The panel GameObject for the search overlay UI
    public Button closeSearchOverlayButton; // Button to close the overlay
    public TMP_InputField searchKeywordInput; // Input field for typing search keywords
    public TMP_Dropdown locationDropdown; // Dropdown for selecting a location filter
    public Button performSearchButton; // Button to trigger the search operation
    public RectTransform suggestionsContainer; // Parent RectTransform for suggestion buttons
    public GameObject suggestionButtonPrefab; // Prefab for individual suggestion buttons


    // --- UI References - Job Details Overlay (NEW) ---
    [Header("UI References - Job Details Overlay")]
    // Assign the root GameObject of the Job Details Overlay Panel here
    public GameObject jobDetailsOverlayPanel;
    // Private reference to the script on the overlay panel
    private JobDetailsOverlayUI jobDetailsOverlayUI;


    // --- Data and Lists ---
    private List<JobData> allJobs = new List<JobData>(); // Stores all jobs loaded from JSON
    private List<JobData> currentlyDisplayedJobs = new List<JobData>(); // Stores jobs currently visible in the list
    private List<GameObject> instantiatedJobCards = new List<GameObject>(); // Tracks instantiated job card GameObjects for cleanup
    private List<GameObject> instantiatedSuggestionButtons = new List<GameObject>(); // Tracks instantiated suggestion button GameObjects for cleanup

    // --- Constants and Configuration ---
    private const string ALL_LOCATIONS_OPTION = "All Locations"; // Text for the "All Locations" dropdown option
    // Hardcoded list of locations for the dropdown.
    // You could also dynamically populate this from job data if needed.
    private readonly List<string> predefinedLocations = new List<string> { "Ho Chi Minh City", "Hanoi" };

    // IMPORTANT: If you have a generic fallback logo INSIDE Assets/Resources/Logos/square-logo/
    // (e.g., "default-logo.png"), set its name here (without the file extension).
    // If you don't have one, leave it empty or comment this line out. The logo will be hidden if not found.
    // Ensure this fallback file exists and is named exactly "default.png" (or whatever you set)
    // in the 'Assets/Resources/Logos/square-logo/' folder and is set as 'Sprite (2D and UI)'.
    private const string FALLBACK_LOGO_FILENAME_INSIDE_SQUARE_LOGO_FOLDER = "default"; // Example: "default" matches "default.png"


    void Start()
    {
        // Load job data from the JSON file in Resources
        LoadJobData();
        // Populate the location filter dropdown based on predefined or loaded data
        PopulateLocationDropdown();

        // Display all jobs initially when the scene starts, if data was loaded
        // Check if allJobs list was successfully populated and is not empty
        if (allJobs != null && allJobs.Count > 0)
        {
            DisplayJobs(allJobs); // Display the full list
        }
        else
        {
            // If no jobs loaded, display an empty list to show the "No jobs found" message
            DisplayJobs(new List<JobData>());
            Debug.LogWarning("No jobs loaded initially or allJobs list is empty after LoadJobData. Displaying empty list with message.");
        }

        // --- UI Event Listener Setup ---
        // Assigning listeners to UI buttons and input fields using null checks for safety

        // Add listener to the button that opens the search overlay
        if (openSearchOverlayButton != null) openSearchOverlayButton.onClick.AddListener(ToggleSearchOverlay);
        else Debug.LogError("OpenSearchOverlayButton not assigned in SearchPageController Inspector.");

        // Hide search overlay initially if assigned
        if (searchOverlayPanel != null) searchOverlayPanel.SetActive(false);
        else Debug.LogError("SearchOverlayPanel not assigned in SearchPageController Inspector."); // This is critical!

        // Add listener to the button that closes the search overlay
        if (closeSearchOverlayButton != null) closeSearchOverlayButton.onClick.AddListener(ToggleSearchOverlay);
        else Debug.LogError("CloseSearchOverlayButton not assigned in SearchPageController Inspector.");

        // Add listener to the button that performs the search
        if (performSearchButton != null) performSearchButton.onClick.AddListener(OnPerformSearch);
        else Debug.LogError("PerformSearchButton not assigned in SearchPageController Inspector.");

        // Add listener to the search keyword input field to trigger suggestions on text change
        if (searchKeywordInput != null) searchKeywordInput.onValueChanged.AddListener(OnSearchKeywordChanged);
        else Debug.LogError("SearchKeywordInput not assigned in SearchPageController Inspector."); // This is critical for suggestions!

        // --- Job Details Overlay Setup (NEW) ---
        if (jobDetailsOverlayPanel != null)
        {
            // Get the JobDetailsOverlayUI script component from the assigned panel
            jobDetailsOverlayUI = jobDetailsOverlayPanel.GetComponent<JobDetailsOverlayUI>();
            if (jobDetailsOverlayUI == null)
            {
                Debug.LogError("JobDetailsOverlayPanel assigned but is missing the JobDetailsOverlayUI script! Please add it.");
            }
            // Ensure the overlay is hidden when the scene starts
            jobDetailsOverlayPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("JobDetailsOverlayPanel not assigned in SearchPageController Inspector. Job details feature (clicking cards) will not work.");
        }
        // --- End Job Details Overlay Setup ---


        // --- Null Reference Checks for Essential UI Elements ---
        // Log errors if critical UI references are not assigned in the Inspector.
        // These won't necessarily stop the script, but they indicate missing setup.
        if (jobCardPrefab == null) Debug.LogError("JobCardPrefab not assigned in SearchPageController Inspector. Job cards cannot be created.");
        if (jobListingsContentArea == null) Debug.LogError("JobListingsContentArea (Scroll View Content) not assigned in SearchPageController Inspector. Jobs cannot be displayed.");
        if (jobCountText == null) Debug.LogError("JobCountText not assigned in SearchPageController Inspector. Job count will not be displayed.");
        if (suggestionsContainer == null) Debug.LogError("SuggestionsContainer not assigned in SearchPageController Inspector. Search suggestions will not appear.");
        if (suggestionButtonPrefab == null) Debug.LogError("SuggestionButtonPrefab not assigned in SearchPageController Inspector. Search suggestions will not function.");
        if (locationDropdown == null) Debug.LogError("LocationDropdown not assigned in SearchPageController Inspector. Location filter will not work.");
        // --- End UI Event Listener Setup & Checks ---
    }

    /// <summary>
    /// Loads job data from the companies.json file located in a Resources folder.
    /// Assumes companies.json is directly inside a folder named 'Resources' anywhere in Assets.
    /// </summary>
    void LoadJobData()
    {
        // Resources.Load path is relative to ANY Assets/Resources folder and does not include the file extension.
        TextAsset jsonFile = Resources.Load<TextAsset>("companies");
        if (jsonFile == null)
        {
            // Log a specific error if the JSON file is not found. Provides the expected path.
            Debug.LogError("Cannot find 'companies.json' in Assets/Resources folder! Make sure the file is exactly at Assets/Resources/companies.json or another Resources folder.");
            allJobs = new List<JobData>(); // Initialize list as empty on failure
            return; // Exit the function
        }

        JobListContainer jobListContainer = null;
        try
        {
            // Attempt to deserialize the JSON string into the JobListContainer object
            jobListContainer = JsonUtility.FromJson<JobListContainer>(jsonFile.text);
        }
        catch (Exception ex)
        {
            // Catch and log detailed error if JSON parsing fails (malformed JSON, incorrect structure etc.)
            Debug.LogError($"Error parsing 'companies.json': {ex.Message}. Ensure JSON is valid and matches the JobData/JobListContainer structure.");
            allJobs = new List<JobData>(); // Initialize list as empty on parse error
            return; // Exit the function
        }

        // Check if deserialization was successful and the 'jobs' array is populated with data
        if (jobListContainer != null && jobListContainer.jobs != null && jobListContainer.jobs.Length > 0)
        {
            // Convert the array from JsonUtility to a List for easier manipulation throughout the script
            allJobs = new List<JobData>(jobListContainer.jobs);
            Debug.Log($"Successfully loaded {allJobs.Count} jobs from companies.json.");
        }
        else
        {
             // Handle cases where parsing was successful but no jobs were found or the JSON structure was unexpected/empty.
            Debug.LogWarning("Loaded companies.json but no jobs were found within the 'jobs' array or the JSON structure is invalid/empty.");
            allJobs = new List<JobData>(); // Ensure allJobs is an empty list if no jobs were found
        }
    }

    /// <summary>
    /// Populates the location dropdown with predefined options and potentially dynamic locations from job data.
    /// </summary>
    void PopulateLocationDropdown()
    {
        if (locationDropdown == null) return; // Exit if dropdown is not assigned

        locationDropdown.ClearOptions(); // Start with a clean list of dropdown options

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData(ALL_LOCATIONS_OPTION)); // Add the default "All Locations" option first

        // Add predefined locations to the options list
        foreach (string loc in predefinedLocations)
        {
            // Avoid adding duplicates if a predefined location somehow appears twice
            if (!options.Any(opt => opt.text.Equals(loc, StringComparison.OrdinalIgnoreCase)))
            {
                 options.Add(new TMP_Dropdown.OptionData(loc));
            }
        }

        // --- Optional: Dynamically add unique locations from job data ---
        // Uncomment this block if you want the dropdown to automatically include
        // any unique location found in your JSON, in addition to predefined ones.
        /*
        if (allJobs != null) // Ensure allJobs is not null before querying
        {
            var jobLocations = allJobs
                               .Where(job => job != null && !string.IsNullOrEmpty(job.location)) // Filter out null jobs and jobs with empty locations
                               .Select(job => job.location.Trim()) // Get the location string and trim whitespace
                               .Distinct() // Get only unique location strings
                               .OrderBy(loc => loc); // Sort locations alphabetically

            foreach (string loc in jobLocations)
            {
                // Add location to options only if it's not already present (case-insensitive check)
                // and not the "All Locations" string itself.
                if (!options.Any(opt => opt.text.Equals(loc, StringComparison.OrdinalIgnoreCase)) &&
                    !loc.Equals(ALL_LOCATIONS_OPTION, StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new TMP_Dropdown.OptionData(loc));
                }
            }
        }
        */
        // --- End Optional Dynamic Location Population ---


        locationDropdown.AddOptions(options); // Add all gathered options to the dropdown
        locationDropdown.RefreshShownValue(); // Ensure the dropdown's visible text is updated correctly
        locationDropdown.value = 0; // Set the default selected value to the first option ("All Locations")
    }

    /// <summary>
    /// Converts a company name string into a sanitized format suitable for a filename.
    /// This function defines the expected filename convention (lowercase, hyphenated, no special chars).
    ///
    /// *** IMPORTANT ACTION REQUIRED IN UNITY EDITOR ***
    /// To load company logos, you MUST ensure the actual image files in your
    /// 'Assets/Resources/Logos/square-logo/' folder are named EXACTLY matching the string
    /// returned by THIS function for the corresponding company, PLUS the '.png' file extension.
    ///
    /// Debugging Tip: Run the scene and check the Unity Console for warnings starting with
    /// "Specific logo not found for company...". These warnings will show you the
    /// "tried path:", which indicates the *exact* filename the script is looking for
    /// based on this sanitization logic. Rename your files in the Project window to match this.
    /// </summary>
    /// <param name="companyName">The original company name string from the JSON.</param>
    /// <returns>A sanitized string like "fpt-software", "zalo-vng-corporation", etc. Returns an empty string if the input is null, empty, or results in an empty string after sanitization.</returns>
    string SanitizeCompanyNameForLogo(string companyName)
    {
        // Return empty string if input is null or empty to avoid errors
        if (string.IsNullOrEmpty(companyName))
            return string.Empty;

        string sanitized = companyName.ToLowerInvariant(); // 1. Convert the entire string to lowercase

        // 2. Replace any character that is *not* a lowercase letter (a-z), a digit (0-9), or a hyphen (-) with a single hyphen.
        // The `+` matches one or more occurrences of the invalid characters.
        // The `\` before the hyphen `\-` is necessary inside the character set `[...]` to treat hyphen literally.
        sanitized = Regex.Replace(sanitized, @"[^a-z0-9\-]+", "-");

        // 3. Replace any sequence of two or more consecutive hyphens (e.g., "--", "---") with a single hyphen.
        sanitized = Regex.Replace(sanitized, @"-+", "-");

        // 4. Remove any leading or trailing hyphens (`-`) that might have been created at the start or end of the string.
        sanitized = sanitized.Trim('-');

        // 5. Final check: if the result of sanitization is an empty string (e.g., if the original name was just punctuation), return empty.
        if (string.IsNullOrEmpty(sanitized))
        {
             // Log a warning if sanitization didn't produce a usable filename
             Debug.LogWarning($"Sanitization of company name '{companyName}' resulted in an empty string. Cannot determine logo filename.");
             return string.Empty; // Return empty string
        }

        // The returned string (e.g., "golden-communication-group") is the name that Resources.Load will look for.
        // You need a file named "golden-communication-group.png" in Assets/Resources/Logos/square-logo/.
        return sanitized;
    }

    /// <summary>
    /// Displays a list of job data in the UI by instantiating Job Card prefabs
    /// for each job and populating their data. Clears previous job cards first.
    /// Also handles displaying a message if the list is empty.
    /// </summary>
    /// <param name="jobsToDisplay">The list of JobData objects to display. Can be null or empty.</param>
    void DisplayJobs(List<JobData> jobsToDisplay)
    {
        // Check if essential UI components are assigned. Log errors if not.
        if (jobListingsContentArea == null || jobCardPrefab == null)
        {
            Debug.LogError("Job listing UI references (Content Area or Prefab) are not set in the Inspector! Cannot display jobs.");
            // Attempt to update count text even if display fails
            if (jobCountText) jobCountText.text = (jobsToDisplay != null && jobsToDisplay.Count >= 0) ? $"{jobsToDisplay.Count} Jobs" : "0 Jobs";
            return; // Exit the function
        }

        // --- Cleanup Previous Cards ---
        // Destroy all GameObjects previously created for job cards
        foreach (GameObject card in instantiatedJobCards)
        {
            // Use null check before destroying, in case an object was somehow already destroyed
            if (card != null)
            {
                Destroy(card);
            }
        }
        instantiatedJobCards.Clear(); // Clear the list after destroying the objects
        // Store the list of jobs that are now being displayed
        currentlyDisplayedJobs = jobsToDisplay;
        // --- End Cleanup ---


        // --- Handle Case: No Jobs Found ---
        // If the input list is null or empty, display a "No jobs found" message
        if (jobsToDisplay == null || jobsToDisplay.Count == 0)
        {
            if (jobCountText) jobCountText.text = "0 Jobs"; // Update the job count text to 0

            // Check if the "No Jobs Found Message" GameObject already exists to prevent duplicates
            // Using jobListingsContentArea.Find() is one way, or you could keep a direct reference.
            if (jobListingsContentArea.Find("NoJobsFoundMessage") == null)
            {
                // Create a new empty GameObject to hold the TextMeshProUGUI component
                GameObject noJobsTextGO = new GameObject("NoJobsFoundMessage");
                // Parent the new GameObject to the content area. 'false' keeps its local position/rotation/scale reset.
                noJobsTextGO.transform.SetParent(jobListingsContentArea, false);
                // Add the TextMeshProUGUI component to the GameObject
                TextMeshProUGUI tmp = noJobsTextGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "No jobs found matching your criteria."; // Set the message text
                tmp.alignment = TextAlignmentOptions.Center; // Center the text horizontally and vertically
                tmp.fontSize = 28; // Set the font size (adjust as needed for your UI)
                tmp.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Set text color (grey with full alpha)
                // You might need to adjust the RectTransform anchor/pivot of noJobsTextGO for proper positioning
                // depending on your layout setup, especially if the Content Area uses a Layout Group.
                // For a simple setup with a Vertical Layout Group, center alignment and a LayoutElement might work.
                // var layoutElement = noJobsTextGO.AddComponent<LayoutElement>();
                // layoutElement.flexibleHeight = 1; // Allows it to take up space in a layout group

                // Add the message GameObject to our tracking list so it gets cleaned up next time DisplayJobs is called
                instantiatedJobCards.Add(noJobsTextGO);
            }
            // Since there are no jobs, we stop here
            return;
        }
        else // --- Case: Jobs Found ---
        {
            // If jobs *are* found, make sure the "No Jobs Found Message" is removed if it was previously displayed
            Transform noJobsMessage = jobListingsContentArea.Find("NoJobsFoundMessage");
            if(noJobsMessage != null)
            {
                // Destroy the "No Jobs Found Message" GameObject
                Destroy(noJobsMessage.gameObject);
                 // Also attempt to remove the reference from our tracking list if it was added
                 instantiatedJobCards.Remove(noJobsMessage.gameObject);
            }
            // Proceed to instantiate and display job cards
        }
        // --- End Handle No Jobs Found ---


        // Update job count text to show the number of jobs currently being displayed
        if (jobCountText) jobCountText.text = $"{jobsToDisplay.Count} Jobs";

        // --- Instantiate and Populate Job Cards ---
        // Loop through each job data object in the list provided
        foreach (JobData job in jobsToDisplay)
        {
            // Skip this iteration if the current job entry is null (shouldn't happen with valid JSON but good defensive coding)
            if (job == null)
            {
                Debug.LogWarning("Encountered a null job entry while attempting to display jobs. Skipping this entry.");
                continue; // Move to the next job in the list
            }

            // Instantiate a new instance of the jobCardPrefab.
            // The second argument (jobListingsContentArea) sets the parent of the new instance.
            GameObject cardInstance = Instantiate(jobCardPrefab, jobListingsContentArea);
            // Add the newly created card instance to our tracking list for later destruction
            instantiatedJobCards.Add(cardInstance);

            // --- Dynamic Logo Loading ---
            // Attempt to find the Image component on a child GameObject named "CompanyLogoImage" within the job card instance.
            Image companyLogoImage = cardInstance.transform.Find("CompanyLogoImage")?.GetComponent<Image>();
            if (companyLogoImage != null) // Check if the Image component was found
            {
                Sprite logoSprite = null; // Initialize the sprite to null
                string sanitizedLogoName = string.Empty; // Initialize sanitized name

                // Attempt to load the specific company logo if the company name is available and not empty
                if (!string.IsNullOrEmpty(job.company))
                {
                    // Get the expected filename based on the company name using the sanitization function
                    sanitizedLogoName = SanitizeCompanyNameForLogo(job.company);

                    // If sanitization produced a valid, non-empty name...
                    if (!string.IsNullOrEmpty(sanitizedLogoName))
                    {
                        // Attempt to load the sprite from the Resources folder.
                        // The path is relative to an Assets/Resources folder and does NOT include the file extension (.png).
                        // e.g., Resources.Load<Sprite>("Logos/square-logo/fpt-software") looks for Assets/Resources/Logos/square-logo/fpt-software.png
                        string logoPath = $"Logos/square-logo/{sanitizedLogoName}";
                        logoSprite = Resources.Load<Sprite>(logoPath);

                        if (logoSprite == null) // If the specific logo was NOT found
                        {
                            // Log a warning message indicating which logo was searched for and not found.
                            // This warning is crucial for debugging and renaming files in the Editor.
                            Debug.LogWarning($"Specific logo not found for company '{job.company}' (tried path: 'Assets/Resources/{logoPath}.png'). " +
                                             $"Ensure a file with this exact name exists in that folder and its Texture Type is 'Sprite (2D and UI)'.");
                        }
                    }
                     // Note: SanitizeCompanyNameForLogo already logs a warning if it results in empty string, so we don't need another here.
                }


                // --- Fallback Logo Logic ---
                // If the specific logo was not found (logoSprite is still null) AND a fallback logo filename is specified...
                if (logoSprite == null && !string.IsNullOrEmpty(FALLBACK_LOGO_FILENAME_INSIDE_SQUARE_LOGO_FOLDER))
                {
                    string fallbackPath = $"Logos/square-logo/{FALLBACK_LOGO_FILENAME_INSIDE_SQUARE_LOGO_FOLDER}";
                     // Attempt to load the generic fallback logo from Resources
                    logoSprite = Resources.Load<Sprite>(fallbackPath);
                    if (logoSprite == null) // If the fallback logo was also not found
                    {
                        Debug.LogWarning($"Fallback logo 'Assets/Resources/{fallbackPath}.png' also not found in Resources. No logo will be displayed for missing specific logos.");
                    }
                }
                // --- End Fallback Logic ---


                // --- Assign Sprite or Hide Image ---
                // If a logo sprite (either specific or fallback) was successfully loaded...
                if (logoSprite != null)
                {
                    companyLogoImage.sprite = logoSprite; // Assign the loaded sprite to the Image component
                    companyLogoImage.gameObject.SetActive(true); // Ensure the Image GameObject is active (visible)

                    // Optional: Use SetNativeSize() if you want the Image to resize to the sprite's dimensions.
                    // This can sometimes interfere with UI Layout Groups, so use with caution or test.
                    // companyLogoImage.SetNativeSize();
                }
                else // If NO logo sprite (neither specific nor fallback) was found...
                {
                    companyLogoImage.gameObject.SetActive(false); // Hide the Image GameObject entirely
                    // Warnings for missing logos were already logged above.
                }
            }
            else // If the "CompanyLogoImage" GameObject or its Image component wasn't found on the prefab instance
            {
                Debug.LogWarning("CompanyLogoImage component not found on JobCardPrefab. Check prefab hierarchy and ensure an active child GameObject named 'CompanyLogoImage' with an Image component exists.");
            }
            // --- End Dynamic Logo Loading ---


            // --- Populate Text Fields (Summary on Card) ---
            // Find each TextMeshProUGUI component by traversing down the hierarchy using Find() and getting the component.
            // Using the ?. (null conditional operator) makes these lookups safer, returning null if a GameObject or component isn't found.
            // Then use a null check (e.g., if (component != null)) before accessing its properties like .text.

            TMP_Text companyNameText = cardInstance.transform.Find("JobDetailsContainer/CompanyNameText")?.GetComponent<TMP_Text>();
            if (companyNameText) companyNameText.text = !string.IsNullOrEmpty(job.company) ? job.company : "Company N/A";
            else Debug.LogWarning($"CompanyNameText not found on JobCardPrefab for job: {job?.company}"); // Log which job failed lookup

            TMP_Text jobTitleText = cardInstance.transform.Find("JobDetailsContainer/JobTitleText")?.GetComponent<TMP_Text>();
            if (jobTitleText) jobTitleText.text = !string.IsNullOrEmpty(job.title) ? job.title : "Title N/A";
            else Debug.LogWarning($"JobTitleText not found on JobCardPrefab for job: {job?.company}");

            TMP_Text locationText = cardInstance.transform.Find("JobDetailsContainer/LocationText")?.GetComponent<TMP_Text>();
            if (locationText) locationText.text = !string.IsNullOrEmpty(job.location) ? $"Location: {job.location}" : "Location: N/A";
            else Debug.LogWarning($"LocationText not found on JobCardPrefab for job: {job?.company}");

            TMP_Text salaryText = cardInstance.transform.Find("JobDetailsContainer/SalaryText")?.GetComponent<TMP_Text>();
            if (salaryText) salaryText.text = !string.IsNullOrEmpty(job.salary) ? $"Salary: {job.salary}" : "Salary: N/A";
            else Debug.LogWarning($"SalaryText not found on JobCardPrefab for job: {job?.company}");

            TMP_Text deadlineText = cardInstance.transform.Find("JobDetailsContainer/DeadlineText")?.GetComponent<TMP_Text>();
            if (deadlineText) deadlineText.text = !string.IsNullOrEmpty(job.deadline) ? $"Deadline: {job.deadline}" : "Deadline: N/A";
            else Debug.LogWarning($"DeadlineText not found on JobCardPrefab for job: {job?.company}");
            // --- End Populate Text Fields ---


            // --- Add Click Listener to Job Card to Show Details Overlay (NEW) ---
            Button cardButton = cardInstance.GetComponent<Button>(); // Try to get an existing Button component
            if (cardButton == null)
            {
                 // If no Button exists on the root, add one dynamically
                 cardButton = cardInstance.AddComponent<Button>();
                 // Optional: Configure Button transition/colors if desired
                 // cardButton.transition = Selectable.Transition.None; // Or ColorTint, SpriteSwap, Animation
                 // Configure the default color tint if using that transition
                 // ColorBlock cb = cardButton.colors;
                 // cb.normalColor = Color.white; // Or whatever your card base color is
                 // cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f); // Slight grey tint on hover/select
                 // cb.pressedColor = new Color(0.7f, 0.7f, 0.7f); // Darker grey when pressed
                 // cardButton.colors = cb;
            }

            // Ensure both the Button component and the overlay script reference are valid
            if (cardButton != null && jobDetailsOverlayUI != null)
            {
                // Remove any old listeners first (good practice if cards could be reused, though here they are destroyed and recreated)
                cardButton.onClick.RemoveAllListeners();

                // Add the new listener. The lambda expression () => { ... } captures the specific 'job' object for this card.
                // When this card is clicked, it will call the Show method on the JobDetailsOverlayUI script, passing *this specific* job data.
                cardButton.onClick.AddListener(() => {
                    jobDetailsOverlayUI.Show(job);
                });
            }
            else if (jobDetailsOverlayUI == null)
            {
                 // Log an error if the overlay script is missing. This warning is logged in Start as well.
                 Debug.LogWarning("JobDetailsOverlayUI script not found. Job cards will be created but will not be clickable to show details.");
            }
            // --- End Add Click Listener ---
        }
        // The job count text was already updated at the beginning of this function if jobs were found.
    }

    /// <summary>
    /// Toggles the active state of the search overlay panel.
    /// When opening, it clears inputs and suggestions and focuses the input field.
    /// </summary>
    void ToggleSearchOverlay()
    {
        if (searchOverlayPanel != null) // Check if the panel GameObject is assigned
        {
            // Determine the target active state (opposite of current)
            bool isActive = !searchOverlayPanel.activeSelf;
            searchOverlayPanel.SetActive(isActive); // Set the panel's active state

            if (isActive) // If the panel was just set to active (it's opening)...
            {
                // Clear any previous search inputs and suggestions
                if (searchKeywordInput != null) searchKeywordInput.text = "";
                if (locationDropdown != null) locationDropdown.value = 0; // Reset location filter to the first option ("All Locations")
                ClearSuggestions(); // Clear any currently displayed suggestions

                // Attempt to select and activate the input field to make it ready for typing immediately
                if (searchKeywordInput != null)
                {
                    searchKeywordInput.Select();
                    // ActivateInputField is often needed for TMP_InputFields to receive keyboard input without an extra click on some platforms/configs
                     searchKeywordInput.ActivateInputField();
                }
            }
            // Optional: Add logic here to disable/enable interaction with the main UI content behind the overlay
        }
        else
        {
             Debug.LogError("SearchOverlayPanel is not assigned in the Inspector. Cannot toggle search overlay.");
        }
    }

     /// <summary>
     /// Performs the search operation based on the current keyword in the input field
     /// and the selected value in the location dropdown. Displays the filtered jobs.
     /// </summary>
    void OnPerformSearch()
    {
        // Get the keyword from the input field, trim leading/trailing whitespace, and convert to lowercase for case-insensitive comparison
        string keyword = "";
        if (searchKeywordInput != null) keyword = searchKeywordInput.text.Trim().ToLower();
        else Debug.LogWarning("SearchKeywordInput is not assigned. Keyword search will be skipped.");


        // Get the selected location from the dropdown. Handle potential nulls or empty dropdown.
        string selectedLocation = "";
        if (locationDropdown != null && locationDropdown.options.Count > 0 && locationDropdown.value >= 0 && locationDropdown.value < locationDropdown.options.Count)
        {
            selectedLocation = locationDropdown.options[locationDropdown.value].text;
        }
        else if (locationDropdown == null)
        {
             Debug.LogWarning("LocationDropdown is not assigned. Location filter will be skipped.");
        }


        // Start the filtering process. Create a copy of all jobs to filter.
        // Ensure allJobs is not null before trying to create a copy.
        List<JobData> filteredJobs = (allJobs != null) ? new List<JobData>(allJobs) : new List<JobData>();

        // Apply keyword filter: If a keyword was entered, keep only jobs where the company name OR title contains the keyword (case-insensitive).
        if (!string.IsNullOrEmpty(keyword))
        {
            filteredJobs = filteredJobs.Where(job =>
                job != null && // Added null check for individual job in case of malformed data
                ((job.company != null && job.company.ToLower().Contains(keyword)) || // Check if company contains keyword
                 (job.title != null && job.title.ToLower().Contains(keyword))) // Check if title contains keyword
            ).ToList(); // Convert the filtered results back to a List
        }

        // Apply location filter: If a specific location is selected (i.e., not "All Locations"),
        // keep only jobs where the location exactly matches the selected location (case-insensitive).
        if (!string.IsNullOrEmpty(selectedLocation) && selectedLocation != ALL_LOCATIONS_OPTION)
        {
            filteredJobs = filteredJobs.Where(job =>
                job != null && // Added null check for individual job
                job.location != null && job.location.Equals(selectedLocation, StringComparison.OrdinalIgnoreCase) // Case-insensitive location comparison
            ).ToList(); // Convert the filtered results back to a List
        }

        // Display the list of jobs that passed the filtering criteria
        DisplayJobs(filteredJobs);

        // Close the search overlay panel after the search is performed, if it is currently active
        if (searchOverlayPanel != null && searchOverlayPanel.activeSelf)
        {
            ToggleSearchOverlay(); // Calling ToggleSearchOverlay when active will set it to inactive
        }
    }

    /// <summary>
    /// Called automatically by the input field when the text content changes.
    /// This function generates and displays search suggestions based on the current input text.
    /// </summary>
    /// <param name="newText">The updated text string in the search keyword input field.</param>
    void OnSearchKeywordChanged(string newText)
    {
        // Clear any suggestions that are currently being displayed from a previous input
        ClearSuggestions();

        // Only generate suggestions if the input text is not empty/whitespace and is at least 2 characters long.
        // This prevents showing suggestions for every single character typed.
        if (string.IsNullOrWhiteSpace(newText) || newText.Length < 2)
        {
            return; // Exit the function if input is too short or empty
        }

        string searchTextLower = newText.ToLower(); // Get the search text in lowercase for case-insensitive matching

        // Generate suggestions: Find jobs where either the company name or job title contains the search text.
        // Then, select those company names and titles as potential suggestions.
        // Ensure allJobs is not null before querying; use empty list if null
        var suggestionsQuery = (allJobs != null ? allJobs : new List<JobData>())
            .Where(job => job != null && // Added null check for individual job for robustness
                          ((job.company != null && job.company.ToLower().Contains(searchTextLower)) || // Check if company name contains the search text
                           (job.title != null && job.title.ToLower().Contains(searchTextLower)))) // Check if job title contains the search text
            // From the matching jobs, select both the company name and the job title strings.
            // SelectMany flattens the result (a list of lists) into a single list of strings.
            .SelectMany(job => new[] { job.company, job.title })
            // Filter out any null results from the selection. Also re-check if the string contains the search text (safety).
             .Where(s => s != null && s.ToLower().Contains(searchTextLower))
            // Get only the unique suggestion strings from the results. Use case-insensitive comparison for distinctness.
            .Distinct(StringComparer.OrdinalIgnoreCase)
            // Order the unique suggestions alphabetically for a clean and predictable display order.
            .OrderBy(s => s)
            // Limit the number of suggestions displayed to prevent the suggestion list from becoming overly long.
            .Take(5); // Example: Display a maximum of 5 suggestions

        // Convert the query results (which are in a deferred execution collection) into a List and pass them to the display function.
        DisplaySuggestions(suggestionsQuery.ToList());
    }

    /// <summary>
    /// Instantiates Button prefabs for each suggestion string in the provided list
    /// and adds them as children to the suggestions container.
    /// </summary>
    /// <param name="suggestions">A list of strings to be used as the text for each suggestion button.</param>
    void DisplaySuggestions(List<string> suggestions)
    {
        // Check if the necessary UI components for displaying suggestions are assigned.
        if (suggestionsContainer == null || suggestionButtonPrefab == null)
        {
            Debug.LogWarning("Suggestions UI references (Container or Prefab) are not set! Cannot display suggestions.");
            return; // Exit the function if components are missing
        }

        // Always clear any previously displayed suggestions before showing new ones.
        ClearSuggestions();

        // If the input list of suggestions is null or empty, there's nothing to display, so exit.
        if (suggestions == null || suggestions.Count == 0)
        {
             // Optional: Ensure the suggestions container is hidden if there are no suggestions.
             // This depends on your UI setup - you might have it always visible or controlled by this.
             // suggestionsContainer.gameObject.SetActive(false);
             return;
        }

        // Optional: Ensure the suggestions container is visible if there *are* suggestions.
        // suggestionsContainer.gameObject.SetActive(true);


        // Loop through each string in the suggestions list
        foreach (string suggestionText in suggestions)
        {
            // Skip if the suggestion text is null or empty, just in case.
            if (string.IsNullOrEmpty(suggestionText)) continue;

            // Instantiate a new instance of the suggestionButtonPrefab.
            // The second argument sets the suggestionsContainer as the parent in the hierarchy.
            GameObject suggestionButtonGO = Instantiate(suggestionButtonPrefab, suggestionsContainer);
            // Add the newly created button GameObject to our tracking list for future cleanup.
            instantiatedSuggestionButtons.Add(suggestionButtonGO);

            // Find the TextMeshProUGUI component within the instantiated button (it might be a child).
            TMP_Text buttonText = suggestionButtonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText) // If the TextMeshProUGUI component was found...
            {
                buttonText.text = suggestionText; // Set the text of the suggestion button.
            }
            else
            {
                // Log a warning if the text component is missing, so the user knows the text won't show.
                Debug.LogWarning("SuggestionButtonPrefab is missing a TextMeshProUGUI child for its text. Suggestion text will not be displayed on the button.");
            }

            // Get the Button component on the instantiated GameObject.
            Button button = suggestionButtonGO.GetComponent<Button>();
            if (button) // If the Button component was found...
            {
                // Add a click listener to the button. This action will be performed when the button is clicked.
                // The lambda expression () => { ... } allows us to use the specific 'suggestionText' for this button.
                button.onClick.AddListener(() => {
                    // When the button is clicked, set the text of the search keyword input field to the suggestion text.
                    if (searchKeywordInput != null) searchKeywordInput.text = suggestionText;
                    // Immediately clear all displayed suggestions after a selection is made.
                    ClearSuggestions();
                    // Optional: You could automatically perform the search here after a suggestion is selected.
                    // OnPerformSearch(); // Uncomment this line to perform search on suggestion click
                });
            }
            else
            {
                // Log a warning if the Button component is missing, as the suggestion won't be clickable.
                Debug.LogWarning("SuggestionButtonPrefab is missing a Button component. Suggestions will be created but will not be clickable.");
            }
        }
    }

    /// <summary>
    /// Destroys all GameObjects currently in the instantiatedSuggestionButtons list
    /// and then clears the list itself. Used to remove suggestions from the UI.
    /// </summary>
    void ClearSuggestions()
    {
        // Check if the suggestions container is assigned. If not, we can't clear suggestions effectively.
        if (suggestionsContainer == null) return;

        // Loop through the list of instantiated suggestion buttons
        foreach (GameObject btn in instantiatedSuggestionButtons)
        {
            // Use null check before destroying, in case an object was somehow already destroyed
            if (btn != null)
            {
                Destroy(btn); // Destroy the GameObject representing the suggestion button
            }
        }
        instantiatedSuggestionButtons.Clear(); // Clear the list reference after destroying the objects

        // Optional: Hide the suggestions container when it's empty after clearing
        // suggestionsContainer.gameObject.SetActive(false); // Depends on your UI setup
    }

    /// <summary>
    /// Cleans up instantiated GameObjects (job cards, suggestion buttons) and removes
    /// event listeners from UI elements when the MonoBehaviour script object is being destroyed.
    /// This helps prevent memory leaks and ensures proper cleanup when the scene changes or the object is removed.
    /// </summary>
    void OnDestroy()
    {
        // Remove listeners from UI elements that might cause errors if the script/object is destroyed but the UI element persists.
        // Check if the UI element reference is not null before attempting to remove listeners.

        if (openSearchOverlayButton != null) openSearchOverlayButton.onClick.RemoveAllListeners();
        if (closeSearchOverlayButton != null) closeSearchOverlayButton.onClick.RemoveAllListeners();
        if (performSearchButton != null) performSearchButton.onClick.RemoveAllListeners();

        // For input fields and dropdowns, check null before removing listeners
        if (searchKeywordInput != null) searchKeywordInput.onValueChanged.RemoveAllListeners();
        // If you added a listener to the locationDropdown, remove it here:
        // if (locationDropdown != null) locationDropdown.onValueChanged.RemoveAllListeners();

        // Note: Listeners on dynamically created buttons (Job Cards, Suggestions) are handled
        // by destroying the GameObjects, which automatically cleans up their listeners.
        // The listeners on the overlay panel buttons are handled by the JobDetailsOverlayUI script's OnDestroy.


        // Destroy any remaining instantiated job cards. This is a cleanup step in case the scene unloads unexpectedly.
        // The DisplayJobs function already handles cleanup before creating new cards, but this is extra safety.
        foreach (GameObject card in instantiatedJobCards)
        {
            if (card != null) Destroy(card); // Check if the GameObject is not null before destroying
        }
        instantiatedJobCards.Clear(); // Clear the list reference

        // Destroy any remaining instantiated suggestion buttons as well.
         foreach (GameObject btn in instantiatedSuggestionButtons)
        {
            if (btn != null) Destroy(btn); // Check if the GameObject is not null before destroying
        }
        instantiatedSuggestionButtons.Clear(); // Clear the list reference
    }
}

// --- Data Structure Classes ---
// !!! IMPORTANT: Define JobData and JobListContainer in their own separate files (e.g., JobData.cs, JobListContainer.cs) !!!
// DO NOT keep these definitions at the bottom of SearchPageController.cs or JobDetailsOverlayUI.cs
// Example structure (ensure this matches your actual classes):

// [System.Serializable]
// public class JobData
// {
//     public string company;
//     public string domain; // Make sure 'domain' exists if you want to open URL
//     public string title;
//     public string location;
//     public string salary;
//     public string experience;
//     public string type;
//     public string description;
//     public bool verified;
//     public string deadline;
// }

// [System.Serializable]
// public class JobListContainer
// {
//     public JobData[] jobs;
// }