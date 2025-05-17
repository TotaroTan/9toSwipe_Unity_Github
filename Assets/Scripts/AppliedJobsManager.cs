using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AppliedJobsManager : MonoBehaviour
{
    public static AppliedJobsManager Instance { get; private set; }

    private const string AppliedJobKeysPrefsKey = "AppliedJobUniqueKeys";
    private List<string> _appliedJobUniqueKeys = new List<string>();
    private List<JobData> _allJobsMasterList;

    void Awake()
    {
        // --- START OF DEBUG LOGS FOR AWAKE ---
        Debug.Log($"[AppliedJobsManager] Awake() called for GameObject: '{gameObject.name}' (Instance ID: {GetInstanceID()})");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[AppliedJobsManager] This is the FIRST instance. Setting Instance and calling DontDestroyOnLoad for '{gameObject.name}'.");

            Debug.Log("[AppliedJobsManager] Proceeding to LoadAllJobsMasterList().");
            LoadAllJobsMasterList(); // Load all job data once

            Debug.Log("[AppliedJobsManager] Proceeding to LoadAppliedJobKeysFromPrefs().");
            LoadAppliedJobKeysFromPrefs(); // Load saved applied job keys
        }
        else if (Instance != this) // If an Instance already exists and it's not this one
        {
            Debug.LogWarning($"[AppliedJobsManager] Another instance of AppliedJobsManager already exists (on '{Instance.gameObject.name}'). Destroying this duplicate GameObject: '{gameObject.name}'.");
            Destroy(gameObject); // Destroy this duplicate instance
        }
        else // Instance == this (shouldn't happen if logic is correct, but good to have a branch)
        {
            Debug.Log($"[AppliedJobsManager] Awake() called again for the existing Instance on '{gameObject.name}'. No action taken.");
        }
        // --- END OF DEBUG LOGS FOR AWAKE ---
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
                Debug.Log($"[AppliedJobsManager] LoadAllJobsMasterList: Loaded {_allJobsMasterList.Count} jobs into master list.");
            }
            else Debug.LogError("[AppliedJobsManager] LoadAllJobsMasterList: Failed to parse JSON or JSON structure is incorrect for master list.");
        }
        else Debug.LogError("[AppliedJobsManager] LoadAllJobsMasterList: Could not load 'companies.json' from Resources for master list!");
    }

    // Generates a unique key for a job.
    private string GenerateUniqueKey(JobData job)
    {
        if (job == null || string.IsNullOrEmpty(job.company) || string.IsNullOrEmpty(job.title))
        {
            return null;
        }
        return $"{job.company.ToLowerInvariant()}_{job.title.ToLowerInvariant()}";
    }

    public void AddAppliedJob(JobData job)
    {
        if (job == null)
        {
            Debug.LogWarning("[AppliedJobsManager] AddAppliedJob called with null job.");
            return;
        }
        string key = GenerateUniqueKey(job);
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning($"[AppliedJobsManager] Could not generate a unique key for the job: {job.company} - {job.title}. Job not added.");
            return;
        }

        Debug.Log($"[AppliedJobsManager] AddAppliedJob attempting for Key: {key}");

        if (!_appliedJobUniqueKeys.Contains(key))
        {
            _appliedJobUniqueKeys.Add(key);
            SaveAppliedJobKeysToPrefs();
            Debug.Log($"[AppliedJobsManager] Job with Key '{key}' ADDED. Total keys now: {_appliedJobUniqueKeys.Count}");
        }
        else
        {
            Debug.Log($"[AppliedJobsManager] Job with Key '{key}' ALREADY EXISTS. Total keys: {_appliedJobUniqueKeys.Count}");
        }
    }

    public void RemoveAppliedJob(JobData job)
    {
        if (job == null) return;
        string key = GenerateUniqueKey(job);
         if (string.IsNullOrEmpty(key)) return;

        if (_appliedJobUniqueKeys.Remove(key))
        {
            SaveAppliedJobKeysToPrefs();
            Debug.Log($"[AppliedJobsManager] Job '{job.title}' by '{job.company}' removed from applied list. Key: {key}");
        }
    }
    public void RemoveAppliedJobByKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (_appliedJobUniqueKeys.Remove(key))
        {
            SaveAppliedJobKeysToPrefs();
            Debug.Log($"[AppliedJobsManager] Job with key '{key}' removed from applied list.");
        }
    }


    public bool IsJobApplied(JobData job)
    {
        if (job == null) return false;
        string key = GenerateUniqueKey(job);
        return !string.IsNullOrEmpty(key) && _appliedJobUniqueKeys.Contains(key);
    }

    public List<JobData> GetAppliedJobsData()
    {
        List<JobData> appliedJobs = new List<JobData>();
        if (_allJobsMasterList == null)
        {
            Debug.LogError("[AppliedJobsManager] GetAppliedJobsData: Master job list not loaded. Cannot retrieve applied jobs data.");
            return appliedJobs; // Return empty list
        }
        if (_appliedJobUniqueKeys == null)
        {
            Debug.LogError("[AppliedJobsManager] GetAppliedJobsData: _appliedJobUniqueKeys list is null. Cannot retrieve applied jobs data.");
            _appliedJobUniqueKeys = new List<string>(); // Initialize to prevent further null errors
            return appliedJobs; // Return empty list
        }


        Debug.Log($"[AppliedJobsManager] GetAppliedJobsData: Checking {_appliedJobUniqueKeys.Count} applied keys against master list of {_allJobsMasterList.Count} jobs.");

        foreach (string key in _appliedJobUniqueKeys)
        {
            JobData foundJob = _allJobsMasterList.FirstOrDefault(j => GenerateUniqueKey(j) == key);
            if (foundJob != null)
            {
                appliedJobs.Add(foundJob);
            }
            else
            {
                Debug.LogWarning($"[AppliedJobsManager] GetAppliedJobsData: Applied job key '{key}' not found in master job list. Data might be stale or key generation mismatch.");
            }
        }
        Debug.Log($"[AppliedJobsManager] GetAppliedJobsData: Returning {appliedJobs.Count} hydrated applied jobs.");
        return appliedJobs;
    }

    private void LoadAppliedJobKeysFromPrefs()
    {
        if (PlayerPrefs.HasKey(AppliedJobKeysPrefsKey))
        {
            string json = PlayerPrefs.GetString(AppliedJobKeysPrefsKey);
            Debug.Log($"[AppliedJobsManager] LoadAppliedJobKeysFromPrefs: Loaded JSON from PlayerPrefs: {json}");
            StringListWrapper wrapper = JsonUtility.FromJson<StringListWrapper>(json);
            if (wrapper != null && wrapper.keys != null)
            {
                _appliedJobUniqueKeys = wrapper.keys;
                Debug.Log($"[AppliedJobsManager] LoadAppliedJobKeysFromPrefs: Successfully parsed. Loaded {_appliedJobUniqueKeys.Count} applied job keys from PlayerPrefs.");
            }
            else
            {
                 _appliedJobUniqueKeys = new List<string>();
                 Debug.LogWarning($"[AppliedJobsManager] LoadAppliedJobKeysFromPrefs: Failed to parse saved applied job keys or wrapper/keys list was null. JSON was: {json}. Initializing empty list.");
            }
        }
        else
        {
            _appliedJobUniqueKeys = new List<string>();
            Debug.Log($"[AppliedJobsManager] LoadAppliedJobKeysFromPrefs: No saved PlayerPrefs key '{AppliedJobKeysPrefsKey}'. Initializing new list.");
        }
    }

    private void SaveAppliedJobKeysToPrefs()
    {
        StringListWrapper wrapper = new StringListWrapper { keys = _appliedJobUniqueKeys };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(AppliedJobKeysPrefsKey, json);
        PlayerPrefs.Save();
        Debug.Log($"[AppliedJobsManager] SaveAppliedJobKeysToPrefs: Saved {_appliedJobUniqueKeys.Count} applied job keys to PlayerPrefs. JSON: {json}");
    }

    [System.Serializable]
    private class StringListWrapper
    {
        public List<string> keys;
    }
}