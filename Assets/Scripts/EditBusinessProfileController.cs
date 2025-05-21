// EditBusinessProfileController.cs
using UnityEngine;
using UnityEngine.UI; // For Button
using TMPro;          // For TextMeshProUGUI and TMP_InputField
// using UnityEngine.SceneManagement; // If you need direct scene loading

public class EditBusinessProfileController : MonoBehaviour
{
    [Header("UI Input Fields")]
    [SerializeField] private TMP_InputField companyNameInput;
    [SerializeField] private TMP_InputField contactPersonNameInput;
    [SerializeField] private TMP_InputField contactJobTitleInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField locationInput;
    [SerializeField] private TMP_InputField passwordInput; // For changing password

    [Header("Buttons")]
    [SerializeField] private Button saveChangesButton;
    [SerializeField] private Button backButton;
    // [SerializeField] private Button editLogoButton; // Placeholder for logo functionality

    // Keys for PlayerPrefs - make them public const so BusinessProfileController can access them
    public const string CompanyNameKey = "BusinessProfile_CompanyName";
    public const string ContactPersonNameKey = "BusinessProfile_ContactPersonName";
    public const string ContactJobTitleKey = "BusinessProfile_ContactJobTitle";
    public const string EmailKey = "BusinessProfile_Email"; // Can share if email is unique, or use BusinessProfile_Email
    public const string LocationKey = "BusinessProfile_Location";
    // Password key would be specific to the business account, e.g., "BusinessProfile_PasswordHash"

    void Start()
    {
        LoadProfileData();

        if (saveChangesButton != null)
        {
            saveChangesButton.onClick.AddListener(OnSaveChangesClicked);
        }

        if (backButton != null)
        {
            // Assuming a ButtonClickHandler or SceneLoader handles navigation
            // e.g., backButton.onClick.AddListener(() => SceneLoader.LoadScene("BusinessProfileScene"));
        }

        // if (editLogoButton != null)
        // {
        //     editLogoButton.onClick.AddListener(OnChangeLogoClicked);
        // }
    }

    void LoadProfileData()
    {
        // Load existing data into input fields
        if (companyNameInput != null) companyNameInput.text = PlayerPrefs.GetString(CompanyNameKey, "Miyamoto");
        if (contactPersonNameInput != null) contactPersonNameInput.text = PlayerPrefs.GetString(ContactPersonNameKey, "Taiyo");
        if (contactJobTitleInput != null) contactJobTitleInput.text = PlayerPrefs.GetString(ContactJobTitleKey, "HR Manager");
        if (emailInput != null) emailInput.text = PlayerPrefs.GetString(EmailKey, "company@gmail.com");
        if (locationInput != null) locationInput.text = PlayerPrefs.GetString(LocationKey, "21"); // Assuming location is a string like street/city

        // Don't load/display saved password for security; let user enter new if they want to change it
        if (passwordInput != null) passwordInput.text = "";
        Debug.Log("Business profile data loaded into edit fields.");
    }

    void OnSaveChangesClicked()
    {
        Debug.Log("Save Changes button clicked for Business Profile!");

        // --- Get data from input fields ---
        string companyName = companyNameInput != null ? companyNameInput.text.Trim() : "";
        string contactPersonName = contactPersonNameInput != null ? contactPersonNameInput.text.Trim() : "";
        string contactJobTitle = contactJobTitleInput != null ? contactJobTitleInput.text.Trim() : "";
        string email = emailInput != null ? emailInput.text.Trim() : "";
        string location = locationInput != null ? locationInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text : ""; // Don't trim password

        // --- Basic Validation (add more as needed) ---
        if (string.IsNullOrWhiteSpace(companyName))
        {
            Debug.LogError("Company name cannot be empty!");
            // TODO: Show UI error to user
            return;
        }
        if (string.IsNullOrWhiteSpace(contactPersonName))
        {
            Debug.LogError("Contact person name cannot be empty!");
            // TODO: Show UI error to user
            return;
        }
        // Add email validation, etc.

        // --- Save Data using PlayerPrefs ---
        PlayerPrefs.SetString(CompanyNameKey, companyName);
        PlayerPrefs.SetString(ContactPersonNameKey, contactPersonName);
        PlayerPrefs.SetString(ContactJobTitleKey, contactJobTitle);
        PlayerPrefs.SetString(EmailKey, email); // Using the same EmailKey for now, ensure uniqueness if needed
        PlayerPrefs.SetString(LocationKey, location);

        // Handle password change (still insecure for MVP, just for demonstration)
        if (!string.IsNullOrWhiteSpace(password))
        {
            Debug.LogWarning("Business: New password entered (INSECURE STORAGE - MVP ONLY!)");
            // For a real app, hash this password before storing or sending to a server.
            // PlayerPrefs.SetString("BusinessProfile_Password_Hashed", YourHashingFunction(password));
        }

        PlayerPrefs.Save(); // Important: actually write data to disk
        Debug.Log("Business profile data saved to PlayerPrefs.");

        // --- Navigation ---
        // Navigate back to the business profile display scene or a confirmation message.
        // This is often handled by a ButtonClickHandler component on the SaveChangesButton
        // or you can explicitly load a scene here.
        // Example: SceneManager.LoadScene("BusinessProfileScene"); // if not handled by button
        Debug.Log("Navigating back to Business Profile Scene after saving (or button click handler will)...");

        // Optionally, call LoadProfileData again if staying on the same scene to refresh fields,
        // though usually you navigate away.
        // LoadProfileData();
    }

    // void OnChangeLogoClicked()
    // {
    //     Debug.Log("Change Business Logo button clicked! Placeholder for file picking logic.");
    //     // TODO: Implement native file picker for image selection here
    // }
}