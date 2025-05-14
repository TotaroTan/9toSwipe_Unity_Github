using System;

[Serializable]
public class JobData // This is your individual job entry class
{
    public string company;
    public string domain;
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
public class JobListContainer // This class holds the array of jobs
{
    public JobData[] jobs; // Field name "jobs" MUST match the key in your JSON
}