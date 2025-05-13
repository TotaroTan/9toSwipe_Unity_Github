using UnityEngine;
using UnityEngine.UI; // For Button
using TMPro;          // For TextMeshProUGUI and TMP_InputField

public class EditProfileController : MonoBehaviour
{
    [Header("UI Input Fields")]
    [SerializeField] private TMP_InputField firstNameInput;
    [SerializeField] private TMP_InputField lastNameInput;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField ageInput; // Changed from phoneInput
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Buttons")]
    [SerializeField] private Button saveChangesButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button editAvatarButton; // For the pencil icon on the avatar

    [Header("Avatar Display (Optional for now)")]
    [SerializeField] private Image profilePicImage; // The main image displaying the avatar

    // Keys for PlayerPrefs - make them public const so UserProfileController can access them
    public const string FirstNameKey = "UserProfile_FirstName";
    public const string LastNameKey = "UserProfile_LastName";
    public const string UsernameKey = "UserProfile_Username";
    public const string EmailKey = "UserProfile_Email";
    public const string AgeKey = "UserProfile_Age"; // Changed from PhoneKey

    void Start()
    {
        LoadProfileData();

        if (saveChangesButton != null)
        {
            saveChangesButton.onClick.AddListener(OnSaveChangesClicked);
        }

        if (backButton != null)
        {
            // Assuming ButtonClickHandler is on backButton and configured to load "UserScene"
            // If not, add: backButton.onClick.AddListener(() => SceneLoader.LoadScene("UserScene"));
        }

        if (editAvatarButton != null)
        {
            editAvatarButton.onClick.AddListener(OnChangeAvatarClicked);
        }
    }

    void LoadProfileData()
    {
        if (firstNameInput != null) firstNameInput.text = PlayerPrefs.GetString(FirstNameKey, "Miyamoto");
        if (lastNameInput != null) lastNameInput.text = PlayerPrefs.GetString(LastNameKey, "Taiyo");
        if (usernameInput != null) usernameInput.text = PlayerPrefs.GetString(UsernameKey, "Taiyoyo");
        if (emailInput != null) emailInput.text = PlayerPrefs.GetString(EmailKey, "Ilovemarketing@gmail.com");
        if (ageInput != null) ageInput.text = PlayerPrefs.GetInt(AgeKey, 19).ToString(); // Load age as Int, display as String

        // Don't load/display saved password for security; let user enter new if they want
        if (passwordInput != null) passwordInput.text = "";
    }

    void OnSaveChangesClicked()
    {
        Debug.Log("Save Changes button clicked!");

        string firstName = firstNameInput != null ? firstNameInput.text.Trim() : "";
        string lastName = lastNameInput != null ? lastNameInput.text.Trim() : "";
        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string email = emailInput != null ? emailInput.text.Trim() : "";
        string ageString = ageInput != null ? ageInput.text.Trim() : "0";
        string password = passwordInput != null ? passwordInput.text : ""; // Don't trim password

        // --- Basic Validation ---
        if (string.IsNullOrWhiteSpace(firstName))
        {
            Debug.LogError("First name cannot be empty!");
            // TODO: Show UI error to user
            return;
        }
        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogError("Username cannot be empty!");
            // TODO: Show UI error to user
            return;
        }

        int age;
        if (!int.TryParse(ageString, out age) || age < 0 || age > 130) // Reasonable age range
        {
            Debug.LogError("Invalid age entered. Please enter a valid number between 0 and 130.");
            // TODO: Show UI error to user
            return;
        }
        // You might want more robust email validation here for a real app

        // --- Save Data ---
        PlayerPrefs.SetString(FirstNameKey, firstName);
        PlayerPrefs.SetString(LastNameKey, lastName);
        PlayerPrefs.SetString(UsernameKey, username);
        PlayerPrefs.SetString(EmailKey, email);
        PlayerPrefs.SetInt(AgeKey, age); // Save age as an integer

        // Handle password change (still insecure for MVP, just for demonstration)
        if (!string.IsNullOrWhiteSpace(password))
        {
            Debug.LogWarning("New password entered (INSECURE STORAGE - MVP ONLY!)");
            // For a real app, hash this password before storing or sending to a server.
            // PlayerPrefs.SetString("UserProfile_Password_Hashed", YourHashingFunction(password));
        }

        PlayerPrefs.Save(); // Important: actually write data to disk
        Debug.Log("Profile data (including age) saved to PlayerPrefs.");

        // Navigation back to UserScene is likely handled by the ButtonClickHandler
        // component on the SaveChangesButton if it's configured to load "UserScene".
        // If not, you would call: SceneLoader.LoadScene("UserScene");
        Debug.Log("Navigating back to UserScene after saving (or button click handler will)...");
    }

    void OnChangeAvatarClicked()
    {
        Debug.Log("Change Avatar button clicked! Placeholder for file picking logic.");
        // TODO: Implement native file picker for image selection here
    }
}