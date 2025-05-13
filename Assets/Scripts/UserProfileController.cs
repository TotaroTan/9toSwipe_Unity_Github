// UserProfileController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserProfileController : MonoBehaviour
{
    [Header("User Info Display UI")]
    [SerializeField] private TextMeshProUGUI nameAndAgeText;
    [SerializeField] private TextMeshProUGUI emailText;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;       // The "RESUME" button on the main profile page
    [SerializeField] private Button avatarButton;

    [Header("Resume Upload Overlay")]
    [SerializeField] private GameObject resumeUploadOverlayPanel; // Drag the ResumeUploadOverlayPanel here
    [SerializeField] private Button chooseFilesButtonInModal;   // The "Choose Files" button INSIDE the modal
    [SerializeField] private Button uploadButtonInModal;        // The "Upload" button INSIDE the modal
    [SerializeField] private Button closeModalButton;           // The "X" or "Close" button INSIDE the modal

    void Start()
    {
        // --- Main Profile Page Button Listeners ---
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OpenResumeUploadOverlay);
        }
        // Avatar button likely uses ButtonClickHandler for scene navigation

        // --- Modal Button Listeners ---
        if (chooseFilesButtonInModal != null)
        {
            chooseFilesButtonInModal.onClick.AddListener(OnChooseFilesClicked);
        }
        if (uploadButtonInModal != null)
        {
            uploadButtonInModal.onClick.AddListener(OnUploadFileClicked);
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
    }

    void OnEnable()
    {
        LoadAndDisplayUserProfile();
    }

    public void LoadAndDisplayUserProfile()
    {
        // ... (your existing code to load and display name, age, email) ...
        string firstName = PlayerPrefs.GetString(EditProfileController.FirstNameKey, "Taiyo");
        string lastName = PlayerPrefs.GetString(EditProfileController.LastNameKey, "Miyamoto");
        int age = PlayerPrefs.GetInt(EditProfileController.AgeKey, 19);
        string email = PlayerPrefs.GetString(EditProfileController.EmailKey, "taiyo1405@gmail.com");

        if (nameAndAgeText != null) nameAndAgeText.text = $"{firstName} {lastName}, {age}";
        if (emailText != null) emailText.text = email;
    }

    // --- Overlay Methods ---
    void OpenResumeUploadOverlay()
    {
        Debug.Log("RESUME button clicked. Opening resume upload overlay.");
        if (resumeUploadOverlayPanel != null)
        {
            resumeUploadOverlayPanel.SetActive(true);
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
        Debug.Log("Modal 'Choose Files' button clicked. Placeholder for native file picker.");
        // TODO: Implement native file picker logic here
        // For MVP, this button doesn't need to do more than log.
    }

    void OnUploadFileClicked()
    {
        Debug.Log("Modal 'Upload' button clicked. Placeholder for file upload logic.");
        // TODO: Implement actual file upload logic here
        // For MVP, after "uploading", you might just close the modal.
        CloseResumeUploadOverlay();
        // You could also update some UI text to say "Resume_Uploaded.pdf"
    }

    // Removed OnUploadResumeClicked as the main resume button now calls OpenResumeUploadOverlay
}