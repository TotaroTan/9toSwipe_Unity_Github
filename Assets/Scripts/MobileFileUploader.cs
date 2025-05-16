using UnityEngine;
using UnityEngine.UI;
using System.IO;
using NativeFilePickerNamespace; // Ensure this is the correct namespace for the plugin

public class MobileFileUploader : MonoBehaviour
{
    public RawImage displayImage; // Assign in Inspector
    public Text feedbackText;     // Assign in Inspector

    public void OnPickFileButtonClicked()
    {
        // According to the README: "PickFile... functions call RequestPermissionAsync internally"
        // So, we don't need to manually check or request permissions before calling PickFile.
        // PickFile itself should handle prompting the user if permissions are needed.

        if (NativeFilePicker.IsFilePickerBusy())
        {
            if (feedbackText) feedbackText.text = "File picker is already busy.";
            Debug.Log("File picker is busy. New request ignored.");
            return;
        }

        if (feedbackText) feedbackText.text = "Opening file picker...";
        Debug.Log("Attempting to pick file. NativeFilePicker should handle permissions.");

        string[] allowedFileTypes = new string[] { "image/png", "application/pdf" };
        // For iOS, you'd use UTIs if being very specific, e.g., "public.png", "com.adobe.pdf"
        // The plugin's ConvertExtensionToFileType can help, or using generic types like "image/*"
        // Let's stick to MIME types for now which work well on Android and are often mapped on iOS.

        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                // This can happen if:
                // 1. User cancels the file picker.
                // 2. Permission was denied by the user when prompted by PickFile's internal request.
                // 3. Permission was already in a 'Denied' state (user selected "Don't ask again" previously).
                if (feedbackText) feedbackText.text = "Operation cancelled or permission denied.";
                Debug.LogWarning("PickFile operation resulted in a null path. User cancelled or permission denied.");

                // At this point, if you want to provide more specific feedback about permanent denial:
                // You *could* call CheckPermission() here to see if it's still false.
                // If CheckPermission() is false, it's more likely a persistent denial.
                bool stillNoPermission = !NativeFilePicker.CheckPermission();
                if (stillNoPermission)
                {
                    if (feedbackText) feedbackText.text += "\nStorage permission may be permanently denied. Check app settings.";
                    Debug.LogWarning("CheckPermission still returns false. User may need to go to app settings.");
                    // You could offer to open settings:
                    // if (NativeFilePicker.CanOpenSettings()) NativeFilePicker.OpenSettings();
                    // Check if CanOpenSettings() and OpenSettings() exist in your plugin version by looking at the README or plugin code.
                    // The README *does* list `NativeFilePicker.OpenSettings()`.
                }
            }
            else
            {
                if (feedbackText) feedbackText.text = "File selected: " + Path.GetFileName(path);
                Debug.Log("Picked file: " + path);

                string extension = Path.GetExtension(path).ToLower();

                if (extension == ".png")
                {
                    LoadPNGFromFile(path);
                }
                else if (extension == ".pdf")
                {
                    HandlePDFFromFile(path);
                }
                else
                {
                    if (feedbackText) feedbackText.text = $"Unsupported file type: {extension}";
                    Debug.LogWarning("Unsupported file type selected: " + path);
                }
            }
        }, allowedFileTypes);
    }

    // PickFileInternal method is no longer strictly needed as a separate method with this simplified logic
    // but the main logic from it is now directly in OnPickFileButtonClicked's PickFile call.

    void LoadPNGFromFile(string path)
    {
        if (feedbackText) feedbackText.text = "Loading PNG...";
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            if (displayImage != null)
            {
                displayImage.texture = tex;
                displayImage.gameObject.SetActive(true);
                if (feedbackText) feedbackText.text = "PNG loaded!";
            }
            else
            {
                if (feedbackText) feedbackText.text = "PNG loaded but no display image assigned.";
            }
        }
        else
        {
            if (feedbackText) feedbackText.text = "Failed to load PNG data.";
            Debug.LogError("Failed to load image: " + path);
        }
    }

    void HandlePDFFromFile(string path)
    {
        if (feedbackText) feedbackText.text = $"PDF selected: {Path.GetFileName(path)}";
        Debug.Log("PDF selected: " + path);
        if (feedbackText) feedbackText.text += "\n(To view, app needs PDF viewer or use external app)";
    }
}