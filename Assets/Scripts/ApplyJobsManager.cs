using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
// using UnityEngine.Networking; // For fetching images if URLs are live
// using System.Collections;     // For Coroutines if fetching images

// Represents the data for a single favorite job
[System.Serializable]
public class FavoriteJobData // This class name can remain as is, or you can rename it
{
    public string companyName;
    public string jobTitle;
    public string logoUrlOrSpritePath; // URL if fetching, or path if using local sprites
    public Sprite localLogoSprite;     // Assign directly if sprites are in project
}

// CLASS NAME MUST MATCH THE FILENAME: _ApplyJobsManager
public class _ApplyJobsManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentPanel;          // Assign the "Content" GameObject of your ScrollView
    public GameObject favoriteJobItemPrefab;    // Assign your "FavoriteJobItem_Template" Prefab
    public TextMeshProUGUI favoritesCountText;  // Assign "SubtitleText_FavoritesCount"

    [Header("Job Data")]
    public List<FavoriteJobData> allFavoriteJobs = new List<FavoriteJobData>(); // Populate this in Inspector or load

    private List<GameObject> instantiatedJobItems = new List<GameObject>();

    void Start()
    {
        if (contentPanel == null || favoriteJobItemPrefab == null || favoritesCountText == null)
        {
            Debug.LogError("_ApplyJobsManager: Crucial UI references are not assigned in the Inspector!");
            enabled = false; // Disable script if setup is incomplete
            return;
        }

        // --- Example Data (Replace or load this dynamically) ---
        if (allFavoriteJobs.Count == 0) // Only add example data if the list is empty
        {
            allFavoriteJobs.Add(new FavoriteJobData { companyName = "Google", jobTitle = "UX Designer", localLogoSprite = null /* Assign Sprite in Inspector or via Resources.Load */ });
            allFavoriteJobs.Add(new FavoriteJobData { companyName = "Meta", jobTitle = "Software Engineer, AI", localLogoSprite = null });
            allFavoriteJobs.Add(new FavoriteJobData { companyName = "Apple", jobTitle = "Hardware Engineer", localLogoSprite = null });
            // ... add more
        }
        // ----------------------------------------------------

        LoadAndDisplayJobs();
    }

    public void LoadAndDisplayJobs()
    {
        // Clear any existing items first
        foreach (GameObject item in instantiatedJobItems)
        {
            Destroy(item);
        }
        instantiatedJobItems.Clear();

        if (allFavoriteJobs == null || allFavoriteJobs.Count == 0)
        {
            UpdateFavoritesCount();
            // Optionally, display a "No jobs" message in the UI
            // e.g., by activating a TextMeshProUGUI element that says "No applied jobs."
            Debug.Log("No favorite/applied jobs to display.");
            return;
        }

        for (int i = 0; i < allFavoriteJobs.Count; i++)
        {
            FavoriteJobData jobData = allFavoriteJobs[i];
            if (jobData == null)
            {
                Debug.LogWarning($"_ApplyJobsManager: Job data at index {i} is null. Skipping.");
                continue;
            }

            GameObject jobItemGO = Instantiate(favoriteJobItemPrefab, contentPanel);
            instantiatedJobItems.Add(jobItemGO);

            // Find components within the instantiated prefab instance
            // Ensure these paths match your "FavoriteJobItem_Template" prefab structure
            Image logoImage = jobItemGO.transform.Find("CompanyLogoImage")?.GetComponent<Image>();
            TextMeshProUGUI companyText = jobItemGO.transform.Find("JobInfoGroup/CompanyNameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI titleText = jobItemGO.transform.Find("JobInfoGroup/JobTitleText")?.GetComponent<TextMeshProUGUI>();
            Button removeButton = jobItemGO.transform.Find("RemoveButton")?.GetComponent<Button>();

            if (companyText != null) companyText.text = jobData.companyName;
            else Debug.LogWarning($"CompanyNameText not found in prefab instance for {jobData.companyName}");

            if (titleText != null) titleText.text = jobData.jobTitle;
            else Debug.LogWarning($"JobTitleText not found in prefab instance for {jobData.companyName}");

            // Handle Logo
            if (logoImage != null)
            {
                if (jobData.localLogoSprite != null)
                {
                    logoImage.sprite = jobData.localLogoSprite;
                    logoImage.enabled = true; // Ensure image component is enabled
                }
                else if (!string.IsNullOrEmpty(jobData.logoUrlOrSpritePath))
                {
                    Sprite loadedSprite = Resources.Load<Sprite>(jobData.logoUrlOrSpritePath);
                    if (loadedSprite != null)
                    {
                        logoImage.sprite = loadedSprite;
                        logoImage.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load sprite from Resources: {jobData.logoUrlOrSpritePath} for {jobData.companyName}. Assign 'Local Logo Sprite' directly or check path.");
                        logoImage.enabled = false; // Hide if no sprite
                    }
                }
                else
                {
                    // Fallback: No sprite or path provided, hide the image or show a placeholder
                    logoImage.enabled = false;
                    Debug.LogWarning($"No logo sprite or path for {jobData.companyName}. Hiding logo image.");
                }
            }
            else Debug.LogWarning($"CompanyLogoImage not found in prefab instance for {jobData.companyName}");


            if (removeButton != null)
            {
                // Capture data for safer removal in the lambda expression
                FavoriteJobData dataToRemove = jobData;
                // Remove any previous listeners to prevent multiple calls if LoadAndDisplayJobs is called again
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() => RemoveJobItem(jobItemGO, dataToRemove));
            }
            else
            {
                Debug.LogError("RemoveButton not found in instantiated job item prefab! Check prefab structure and naming.");
            }
        }
        UpdateFavoritesCount();
    }

    public void RemoveJobItem(GameObject itemGameObject, FavoriteJobData jobDataToRemove)
    {
        if (itemGameObject != null)
        {
            instantiatedJobItems.Remove(itemGameObject);
            Destroy(itemGameObject); // Remove from UI

            // Remove from the data source
            if (allFavoriteJobs.Contains(jobDataToRemove))
            {
                allFavoriteJobs.Remove(jobDataToRemove);
                Debug.Log($"Removed job: {jobDataToRemove.companyName} - {jobDataToRemove.jobTitle}");
            }
            else
            {
                Debug.LogWarning($"Job data for {jobDataToRemove.companyName} not found in allFavoriteJobs list during removal.");
            }

            UpdateFavoritesCount();
        }
    }

    void UpdateFavoritesCount()
    {
        if (favoritesCountText != null)
        {
            int count = allFavoriteJobs.Count;
            favoritesCountText.text = $"{count} Job{(count != 1 ? "s" : "")}";
        }
    }

    // Optional: Coroutine for loading image from URL
    // IEnumerator LoadImageFromURL(string url, Image targetImage) { ... }

    // Optional: Coroutine for fade out animation (requires CanvasGroup on prefab)
    // IEnumerator FadeOutAndRemove(GameObject itemGameObject, FavoriteJobData jobDataToRemove) { ... }
}