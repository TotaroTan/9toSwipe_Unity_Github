// MobileFileUploader.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
// REMOVED: using NativeFilePickerNamespace; // This was incorrect

public class MobileFileUploader : MonoBehaviour
{
    public RawImage displayImage; // Assign in Inspector
    public Text feedbackText;     // Assign in Inspector

    public void OnPickFileButtonClicked()
    {
        if (NativeFilePicker.IsFilePickerBusy()) // Correct: Call static method directly
        {
            if (feedbackText) feedbackText.text = "File picker is already busy.";
            Debug.Log("File picker is busy. New request ignored.");
            return;
        }

        if (feedbackText) feedbackText.text = "Opening file picker...";
        Debug.Log("Attempting to pick file. NativeFilePicker should handle permissions.");

        // Define allowed file types using MIME types for broader compatibility
        // The NativeFilePicker script handles platform-specific conversion or usage.
        string[] allowedFileTypes = new string[] { "image/png", "image/jpeg", "application/pdf" };

        NativeFilePicker.PickFile((path) => // Correct: Call static method directly
        {
            if (path == null)
            {
                if (feedbackText) feedbackText.text = "Operation cancelled or permission denied.";
                Debug.LogWarning("PickFile operation resulted in a null path. User cancelled or permission denied.");

                // Check if permission is still denied (might be permanently)
                if (!NativeFilePicker.CheckPermission(true)) // Check read permission
                {
                    if (feedbackText) feedbackText.text += "\nStorage permission may be denied. Check app settings.";
                    Debug.LogWarning("CheckPermission still returns false. User may need to go to app settings.");
                    // Optionally offer to open settings if the plugin supports it and it makes sense for your UX
                    // if (NativeFilePicker.CanOpenSettings()) // Assuming CanOpenSettings is a valid method
                    // {
                    //     NativeFilePicker.OpenSettings();
                    // }
                }
            }
            else
            {
                if (feedbackText) feedbackText.text = "File selected: " + Path.GetFileName(path);
                Debug.Log("Picked file: " + path);

                string extension = Path.GetExtension(path).ToLowerInvariant();

                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                {
                    LoadImageFromFile(path); // Generalized for common image types
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

    void LoadImageFromFile(string path)
    {
        if (feedbackText) feedbackText.text = "Loading image...";
        if (!File.Exists(path))
        {
            if (feedbackText) feedbackText.text = "Error: File not found at " + path;
            Debug.LogError("File not found: " + path);
            return;
        }

        byte[] fileData;
        try
        {
            fileData = File.ReadAllBytes(path);
        }
        catch (System.Exception e)
        {
            if (feedbackText) feedbackText.text = "Error reading file.";
            Debug.LogError("Failed to read file bytes: " + path + "\n" + e.Message);
            return;
        }

        Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; LoadImage will resize it
        if (tex.LoadImage(fileData)) // LoadImage auto-detects JPG/PNG
        {
            if (displayImage != null)
            {
                displayImage.texture = tex;
                displayImage.gameObject.SetActive(true); // Ensure RawImage is visible
                if (feedbackText) feedbackText.text = "Image loaded!";
            }
            else
            {
                if (feedbackText) feedbackText.text = "Image loaded but no display image assigned.";
                Destroy(tex); // Clean up texture if not used
            }
        }
        else
        {
            if (feedbackText) feedbackText.text = "Failed to load image data into texture.";
            Debug.LogError("Failed to load image into texture: " + path);
            Destroy(tex); // Clean up texture on failure
        }
    }

    void HandlePDFFromFile(string path)
    {
        if (feedbackText) feedbackText.text = $"PDF selected: {Path.GetFileName(path)}";
        Debug.Log("PDF selected: " + path);
        // Actual PDF handling (e.g., uploading, or opening with an external app) would go here.
        // For display within Unity, you'd need a dedicated PDF rendering asset.
        if (feedbackText) feedbackText.text += "\n(To view, app needs PDF viewer or use external app)";
    }
}