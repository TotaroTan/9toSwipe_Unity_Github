// UserProfileController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO; // Required for Path.GetFileName
// REMOVED: using NativeFilePickerNamespace; // This was incorrect

public class UserProfileController : MonoBehaviour
{
    [Header("User Info Display UI")]
    [SerializeField] private TextMeshProUGUI nameAndAgeText;
    [SerializeField] private TextMeshProUGUI emailText;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button avatarButton; // Assuming you might use this for avatar picking later

    [Header("Resume Upload Overlay")]
    [SerializeField] private GameObject resumeUploadOverlayPanel;
    [SerializeField] private Button chooseFilesButtonInModal;
    [SerializeField] private Button uploadButtonInModal;
    [SerializeField] private Button closeModalButton;
    [SerializeField] private TextMeshProUGUI chosenFileNameTextInModal;
    [SerializeField] private TextMeshProUGUI modalFeedbackText;

    private string pickedFilePath = null;
    private string currentlyUploadedResumeName = "RESUME";

    private const string UploadedResumeNameKey = "UserUploadedResumeName";
    private const string UploadedResumePathKey = "UserUploadedResumePath";

    void Start()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OpenResumeUploadOverlay);
        }
        // if (avatarButton != null) avatarButton.onClick.AddListener(OnPickAvatarClicked); // Example for avatar

        if (chooseFilesButtonInModal != null)
        {
            chooseFilesButtonInModal.onClick.AddListener(OnChooseFilesClicked);
        }
        if (uploadButtonInModal != null)
        {
            uploadButtonInModal.onClick.AddListener(OnUploadFileClicked);
            uploadButtonInModal.interactable = false;
        }
        if (closeModalButton != null)
        {
            closeModalButton.onClick.AddListener(CloseResumeUploadOverlay);
        }

        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(false);
        }

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
                buttonText.text = currentlyUploadedResumeName.ToUpperInvariant(); // Example: Make it all caps
            }
        }
    }

    void OpenResumeUploadOverlay()
    {
        Debug.Log("RESUME button clicked. Opening resume upload overlay.");
        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(true);
            pickedFilePath = null;
            if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen.";
            if (uploadButtonInModal != null) uploadButtonInModal.interactable = false;
            if (modalFeedbackText != null) modalFeedbackText.text = "";
        }
    }

    void CloseResumeUploadOverlay()
    {
        Debug.Log("Closing resume upload overlay.");
        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(false);
        }
    }

    void OnChooseFilesClicked()
    {
        if (NativeFilePicker.IsFilePickerBusy()) // Correct: Call static method
        {
            Debug.LogWarning("NativeFilePicker is already busy.");
            if (modalFeedbackText != null) modalFeedbackText.text = "File picker is busy. Try again.";
            return;
        }

        // Define allowed file types (PDFs and common images)
        // The NativeFilePicker.cs script itself handles how these translate to platform specifics
        string[] allowedFileTypes = new string[] { "application/pdf", "image/png", "image/jpeg" };

        NativeFilePicker.PickFile((path) => // Correct: Call static method
        {
            if (path == null)
            {
                Debug.Log("File pick operation cancelled or failed.");
                // Optionally clear feedback or reset UI if user cancels
                // if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen.";
                // if (uploadButtonInModal != null) uploadButtonInModal.interactable = false;
                if (modalFeedbackText != null) modalFeedbackText.text = "File selection cancelled.";

                // Check if permission is still denied (might be permanently)
                if (!NativeFilePicker.CheckPermission(true)) // Check read permission
                {
                     if (modalFeedbackText != null) modalFeedbackText.text += "\nStorage permission may be denied.";
                }
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
                uploadButtonInModal.interactable = true;
            }
            if (modalFeedbackText != null) modalFeedbackText.text = "File ready to upload.";


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
        PlayerPrefs.SetString(UploadedResumeNameKey, fileName);
        PlayerPrefs.SetString(UploadedResumePathKey, pickedFilePath);
        PlayerPrefs.Save();

        currentlyUploadedResumeName = fileName;
        UpdateResumeButtonText();
        // -----------------------------------------

        if (modalFeedbackText != null) modalFeedbackText.text = $"{fileName} uploaded successfully!";
        if (uploadButtonInModal != null) uploadButtonInModal.interactable = false; // Disable after upload
        if (chosenFileNameTextInModal != null) chosenFileNameTextInModal.text = "No file chosen."; // Reset

        Invoke(nameof(CloseResumeUploadOverlayAfterSuccess), 1.5f);
    }

    void CloseResumeUploadOverlayAfterSuccess()
    {
        CloseResumeUploadOverlay();
    }

    // Example for Avatar Picking (if you add it later)
    // public void OnPickAvatarClicked()
    // {
    //     if (NativeFilePicker.IsFilePickerBusy()) return;
    //     NativeFilePicker.PickFile((path) =>
    //     {
    //         if (path != null)
    //         {
    //             Debug.Log("Avatar image picked: " + path);
    //             // Load image and set it to an avatar RawImage UI element
    //             // PlayerPrefs.SetString("UserAvatarPath", path); PlayerPrefs.Save();
    //         }
    //     }, new string[] { "image/png", "image/jpeg" });
    // }
}