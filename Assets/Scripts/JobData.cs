using System;

[Serializable]
public class JobData
{
    public string company;
    public string domain; // Keep domain if you might use it later for logos
    public string title;
    public string location;
    public string salary;
    public string experience;
    public string type;
    public string description;
    public bool verified;
    public string deadline;
}

[Serializable]
public class JobListContainer // Changed name to avoid conflict
{
    public JobData[] jobs; // Field name MUST match the key in JSON ("jobs")
}