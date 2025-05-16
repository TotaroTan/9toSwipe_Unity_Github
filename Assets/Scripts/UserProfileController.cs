using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO; // Required for Path.GetFileName

// Make sure you have this using statement for the NativeFilePicker plugin
using NativeFilePickerNamespace;

public class UserProfileController : MonoBehaviour
{
    [Header("User Info Display UI")]
    [SerializeField] private TextMeshProUGUI nameAndAgeText;
    [SerializeField] private TextMeshProUGUI emailText;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;       // The "RESUME" button on the main profile page
    [SerializeField] private Button avatarButton;

    [Header("Resume Upload Overlay")]
    [SerializeField] private GameObject resumeUploadOverlayPanel;
    [SerializeField] private Button chooseFilesButtonInModal;
    [SerializeField] private Button uploadButtonInModal;
    [SerializeField] private Button closeModalButton;
    [SerializeField] private TextMeshProUGUI chosenFileNameTextInModal; // UI to display the name of the chosen file
    [SerializeField] private TextMeshProUGUI modalFeedbackText;       // Optional: For messages like "Please choose a file"

    private string pickedFilePath = null; // To store the path of the file chosen by the user
    private string currentlyUploadedResumeName = "RESUME"; // Default text for the resume button

    // Key for PlayerPrefs to store the name of the uploaded resume
    private const string UploadedResumeNameKey = "UserUploadedResumeName";
    private const string UploadedResumePathKey = "UserUploadedResumePath"; // If you want to store the path

    void Start()
    {
        // --- Main Profile Page Button Listeners ---
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OpenResumeUploadOverlay);
        }

        // --- Modal Button Listeners ---
        if (chooseFilesButtonInModal != null)
        {
            chooseFilesButtonInModal.onClick.AddListener(OnChooseFilesClicked);
        }
        if (uploadButtonInModal != null)
        {
            uploadButtonInModal.onClick.AddListener(OnUploadFileClicked);
            uploadButtonInModal.interactable = false; // Disable upload until a file is chosen
        }
        if (closeModalButton != null)
        {
            closeModalButton.onClick.AddListener(CloseResumeUploadOverlay);
        }

        // Ensure overlay is hidden at start
        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(false);
        }

        // Load the name of any previously "uploaded" resume
        currentlyUploadedResumeName = PlayerPrefs.GetString(UploadedResumeNameKey, "RESUME");
        UpdateResumeButtonText();
    }

    void OnEnable()
    {
        LoadAndDisplayUserProfile();
    }

    public void LoadAndDisplayUserProfile()
    {
        string firstName = PlayerPrefs.GetString(EditProfileController.FirstNameKey, "Taiyo");
        string lastName = PlayerPrefs.GetString(EditProfileController.LastNameKey, "Miyamoto");
        int age = PlayerPrefs.GetInt(EditProfileController.AgeKey, 19);
        string email = PlayerPrefs.GetString(EditProfileController.EmailKey, "taiyo1405@gmail.com");

        if (nameAndAgeText != null) nameAndAgeText.text = $"{firstName} {lastName}, {age}";
        if (emailText != null) emailText.text = email;
    }

    void UpdateResumeButtonText()
    {
        if (resumeButton != null)
        {
            TextMeshProUGUI buttonText = resumeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = currentlyUploadedResumeName;
            }
        }
    }

    // --- Overlay Methods ---
    void OpenResumeUploadOverlay()
    {
        Debug.Log("RESUME button clicked. Opening resume upload overlay.");
        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(true);
            pickedFilePath = null; // Reset picked file path when opening
            if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen.";
            if (uploadButtonInModal != null) uploadButtonInModal.interactable = false; // Disable upload button
            if (modalFeedbackText != null) modalFeedbackText.text = ""; // Clear feedback
        }
    }

    void CloseResumeUploadOverlay()
    {
        Debug.Log("Closing resume upload overlay.");
        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(false);
        }
        // Optionally reset pickedFilePath if you don't want it to persist if modal is re-opened
        // pickedFilePath = null;
        // if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen.";
        // if (uploadButtonInModal != null) uploadButtonInModal.interactable = false;
    }

    void OnChooseFilesClicked()
    {
        if (NativeFilePicker.IsFilePickerBusy())
        {
            Debug.LogWarning("NativeFilePicker is already busy.");
            if (modalFeedbackText != null) modalFeedbackText.text = "File picker is busy. Try again.";
            return;
        }

        // Define allowed file types (PDFs and common images)
        string[] allowedFileTypes;
        #if UNITY_ANDROID
            allowedFileTypes = new string[] { "image/*", "application/pdf" };
        #elif UNITY_IOS
            allowedFileTypes = new string[] { "public.image", "com.adobe.pdf" }; // UTIs for iOS
        #else
            allowedFileTypes = null; // No specific filtering for editor/other platforms
            Debug.LogWarning("NativeFilePicker: Platform not Android or iOS. File type filtering may not behave as expected.");
        #endif

        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("File pick operation cancelled or failed.");
                // pickedFilePath remains null or its previous value
                // If you want to clear previous selection on cancel:
                // pickedFilePath = null;
                // if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen.";
                // if (uploadButtonInModal != null) uploadButtonInModal.interactable = false;
                if (modalFeedbackText != null) modalFeedbackText.text = "File selection cancelled.";
                return;
            }

            Debug.Log("File picked: " + path);
            pickedFilePath = path;

            if (chosenFileNameTextInModal != null)
            {
                chosenFileNameTextInModal.text = "Selected: " + Path.GetFileName(path);
            }
            if (uploadButtonInModal != null)
            {
                uploadButtonInModal.interactable = true; // Enable upload button
            }
            if (modalFeedbackText != null) modalFeedbackText.text = ""; // Clear feedback


        }, allowedFileTypes);
    }

    void OnUploadFileClicked()
    {
        if (string.IsNullOrEmpty(pickedFilePath))
        {
            Debug.LogWarning("Upload clicked, but no file was chosen.");
            if (modalFeedbackText != null) modalFeedbackText.text = "Please choose a file first.";
            return;
        }

        Debug.Log($"Modal 'Upload' button clicked. Simulating upload for: {pickedFilePath}");
        string fileName = Path.GetFileName(pickedFilePath);

        // --- Actual "Upload" Logic Placeholder ---
        // For now, we'll just store the file name to display on the main resume button
        // and potentially the path if you need to access it later (e.g., to open it)
        PlayerPrefs.SetString(UploadedResumeNameKey, fileName);
        PlayerPrefs.SetString(UploadedResumePathKey, pickedFilePath); // Store the actual path
        PlayerPrefs.Save();

        currentlyUploadedResumeName = fileName;
        UpdateResumeButtonText();
        // -----------------------------------------

        if (modalFeedbackText != null) modalFeedbackText.text = $"{fileName} uploaded successfully!";

        // Close the modal after a short delay to show success message (optional)
        Invoke(nameof(CloseResumeUploadOverlayAfterSuccess), 1.5f);
        // Or close immediately:
        // CloseResumeUploadOverlay();

        // You might want to clear pickedFilePath after "uploading"
        // pickedFilePath = null;
        // if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen.";
        // if (uploadButtonInModal != null) uploadButtonInModal.interactable = false;
    }

    void CloseResumeUploadOverlayAfterSuccess()
    {
        CloseResumeUploadOverlay();
    }
}