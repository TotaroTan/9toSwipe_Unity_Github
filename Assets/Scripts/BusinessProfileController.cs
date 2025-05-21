// BusinessProfileController.cs
using UnityEngine;
using UnityEngine.UI; // If you add buttons later
using TMPro;          // For TextMeshProUGUI

public class BusinessProfileController : MonoBehaviour
{
    [Header("Business Info Display UI")]
    [SerializeField] private TextMeshProUGUI companyAndContactText; // e.g., "Pizza Company, Harry"
    [SerializeField] private TextMeshProUGUI contactJobTitleText;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TextMeshProUGUI locationText;

    // You might add buttons for "Edit Profile", "View Job Postings", etc. later
    // [Header("Buttons")]
    // [SerializeField] private Button editProfileButton;

    void Start()
    {
        // Example: Hook up an edit button if you add one
        // if (editProfileButton != null)
        // {
        //     editProfileButton.onClick.AddListener(() => SceneLoader.LoadScene("EditBusinessProfileScene")); // Assuming a SceneLoader script
        // }
    }

    void OnEnable() // Load data when the panel/scene becomes active
    {
        LoadAndDisplayBusinessProfile();
    }

    public void LoadAndDisplayBusinessProfile()
    {
        // Retrieve data saved by EditBusinessProfileController
        string companyName = PlayerPrefs.GetString(EditBusinessProfileController.CompanyNameKey, "Default Company Inc.");
        string contactPersonName = PlayerPrefs.GetString(EditBusinessProfileController.ContactPersonNameKey, "John Doe");
        string contactJobTitle = PlayerPrefs.GetString(EditBusinessProfileController.ContactJobTitleKey, "HR Manager");
        string email = PlayerPrefs.GetString(EditBusinessProfileController.EmailKey, "contact@defaultcompany.com");
        string location = PlayerPrefs.GetString(EditBusinessProfileController.LocationKey, "Default City");

        // --- Display the data ---
        if (companyAndContactText != null)
        {
            companyAndContactText.text = $"{companyName}, {contactPersonName}";
        }
        if (contactJobTitleText != null)
        {
            contactJobTitleText.text = contactJobTitle;
        }
        if (emailText != null)
        {
            emailText.text = email;
        }
        if (locationText != null)
        {
            locationText.text = location;
        }

        Debug.Log("Business profile data loaded and displayed.");
    }
}