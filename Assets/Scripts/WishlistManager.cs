using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WishlistManager : MonoBehaviour
{
    public static WishlistManager Instance { get; private set; }

    private const string WishlistedJobsPrefsKey = "WishlistedJobsWithStatus"; // PlayerPrefs key
    private List<WishlistedJobEntry> _wishlistedJobs = new List<WishlistedJobEntry>(); // Stores key and status

    // To easily retrieve full JobData later for the Wishlist page
    private List<JobData> _allJobsMasterList;
    private System.Random _random = new System.Random();
    private string[] _possibleStatuses = { "Closed", "Pending", "Ready" };

    // Helper class for PlayerPrefs serialization
    [System.Serializable]
    private class WishlistedJobEntry
    {
        public string uniqueKey;
        public string status;
    }

    [System.Serializable]
    private class WishlistWrapper
    {
        public List<WishlistedJobEntry> entries;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllJobsMasterList();
            LoadWishlistedJobsFromPrefs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllJobsMasterList()
    {
        _allJobsMasterList = new List<JobData>();
        TextAsset jsonFile = Resources.Load<TextAsset>("companies");
        if (jsonFile != null)
        {
            JobListContainer jobListContainer = JsonUtility.FromJson<JobListContainer>(jsonFile.text);
            if (jobListContainer != null && jobListContainer.jobs != null)
            {
                _allJobsMasterList.AddRange(jobListContainer.jobs);
                Debug.Log($"[WishlistManager] Loaded {_allJobsMasterList.Count} jobs into master list.");
            }
            else Debug.LogError("[WishlistManager] Failed to parse JSON or JSON structure is incorrect for master list.");
        }
        else Debug.LogError("[WishlistManager] Could not load 'companies.json' from Resources for master list!");
    }

    private string GenerateUniqueKey(JobData job)
    {
        if (job == null || string.IsNullOrEmpty(job.company) || string.IsNullOrEmpty(job.title))
        {
            return null;
        }
        return $"{job.company.ToLowerInvariant()}_{job.title.ToLowerInvariant()}";
    }

    private string GetRandomStatus()
    {
        int index = _random.Next(_possibleStatuses.Length);
        return _possibleStatuses[index];
    }

    public void AddToWishlist(JobData job)
    {
        if (job == null) return;
        string key = GenerateUniqueKey(job);
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[WishlistManager] Could not generate unique key. Job not added to wishlist.");
            return;
        }

        if (!_wishlistedJobs.Any(wj => wj.uniqueKey == key))
        {
            string randomStatus = GetRandomStatus();
            _wishlistedJobs.Add(new WishlistedJobEntry { uniqueKey = key, status = randomStatus });
            SaveWishlistedJobsToPrefs();
            Debug.Log($"[WishlistManager] Job '{job.title}' by '{job.company}' (Key: {key}) added to wishlist with status: {randomStatus}");
        }
        else
        {
            Debug.Log($"[WishlistManager] Job '{job.title}' by '{job.company}' (Key: {key}) is already in the wishlist.");
        }
    }

    public void RemoveFromWishlist(JobData job)
    {
        if (job == null) return;
        string key = GenerateUniqueKey(job);
        if (string.IsNullOrEmpty(key)) return;

        int removedCount = _wishlistedJobs.RemoveAll(wj => wj.uniqueKey == key);
        if (removedCount > 0)
        {
            SaveWishlistedJobsToPrefs();
            Debug.Log($"[WishlistManager] Job '{job.title}' by '{job.company}' (Key: {key}) removed from wishlist.");
        }
    }
     public void RemoveFromWishlistByKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        int removedCount = _wishlistedJobs.RemoveAll(wj => wj.uniqueKey == key);
        if (removedCount > 0)
        {
            SaveWishlistedJobsToPrefs();
            Debug.Log($"[WishlistManager] Job with key '{key}' removed from wishlist.");
        }
    }


    public bool IsJobInWishlist(JobData job)
    {
        if (job == null) return false;
        string key = GenerateUniqueKey(job);
        return !string.IsNullOrEmpty(key) && _wishlistedJobs.Any(wj => wj.uniqueKey == key);
    }

    public List<WishlistedJobData> GetWishlistedJobsData()
    {
        List<WishlistedJobData> result = new List<WishlistedJobData>();
        if (_allJobsMasterList == null)
        {
            Debug.LogError("[WishlistManager] Master job list not loaded. Cannot retrieve wishlisted jobs data.");
            return result;
        }

        foreach (WishlistedJobEntry entry in _wishlistedJobs)
        {
            JobData foundJobDetails = _allJobsMasterList.FirstOrDefault(j => GenerateUniqueKey(j) == entry.uniqueKey);
            if (foundJobDetails != null)
            {
                result.Add(new WishlistedJobData(foundJobDetails, entry.status));
            }
            else
            {
                Debug.LogWarning($"[WishlistManager] Wishlisted job key '{entry.uniqueKey}' not found in master job list. Data might be stale.");
            }
        }
        return result;
    }

    private void LoadWishlistedJobsFromPrefs()
    {
        if (PlayerPrefs.HasKey(WishlistedJobsPrefsKey))
        {
            string json = PlayerPrefs.GetString(WishlistedJobsPrefsKey);
            WishlistWrapper wrapper = JsonUtility.FromJson<WishlistWrapper>(json);
            if (wrapper != null && wrapper.entries != null)
            {
                _wishlistedJobs = wrapper.entries;
                Debug.Log($"[WishlistManager] Loaded {_wishlistedJobs.Count} wishlisted jobs from PlayerPrefs.");
            }
            else
            {
                _wishlistedJobs = new List<WishlistedJobEntry>();
                Debug.LogWarning("[WishlistManager] Failed to parse saved wishlisted jobs or list was null.");
            }
        }
        else
        {
            _wishlistedJobs = new List<WishlistedJobEntry>();
             Debug.Log("[WishlistManager] No saved wishlisted jobs found in PlayerPrefs. Initializing new list.");
        }
    }

    private void SaveWishlistedJobsToPrefs()
    {
        WishlistWrapper wrapper = new WishlistWrapper { entries = _wishlistedJobs };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(WishlistedJobsPrefsKey, json);
        PlayerPrefs.Save();
        Debug.Log($"[WishlistManager] Saved {_wishlistedJobs.Count} wishlisted jobs to PlayerPrefs.");
    }
}