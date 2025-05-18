using System;

[Serializable]
public class WishlistedJobData
{
    public JobData jobDetails; // The original job data
    public string status;      // "Closed", "Pending", or "Ready"

    // Constructor for convenience
    public WishlistedJobData(JobData details, string jobStatus)
    {
        jobDetails = details;
        status = jobStatus;
    }
}