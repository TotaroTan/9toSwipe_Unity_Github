using UnityEngine;
using UnityEngine.UI; // Required for Button

public class SimpleOverlayController : MonoBehaviour
{
    [Header("Main Button")]
    [Tooltip("The button that will open the overlay.")]
    public Button openOverlayButton; // Assign your main button here

    [Header("Overlay Elements")]
    [Tooltip("The parent GameObject of your overlay panel.")]
    public GameObject overlayPanel;   // Assign your overlay panel here
    [Tooltip("The button inside the overlay panel that will close it.")]
    public Button closeOverlayButton; // Assign the close button from within the overlay here

    void Start()
    {
        // Ensure the overlay is hidden when the game starts
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Overlay Panel is not assigned in the Inspector!");
            // Optionally disable this script or the open button if the overlay isn't set up
            if (openOverlayButton != null) openOverlayButton.interactable = false;
            enabled = false; // Disable this script component
            return;
        }

        // Add a listener to the openOverlayButton
        // This means when openOverlayButton is clicked, the OpenOverlay method will be called
        if (openOverlayButton != null)
        {
            openOverlayButton.onClick.AddListener(OpenOverlay);
        }
        else
        {
            Debug.LogError("Open Overlay Button is not assigned in the Inspector!");
        }

        // Add a listener to the closeOverlayButton
        // This means when closeOverlayButton is clicked, the CloseOverlay method will be called
        if (closeOverlayButton != null)
        {
            closeOverlayButton.onClick.AddListener(CloseOverlay);
        }
        else
        {
            Debug.LogError("Close Overlay Button is not assigned in the Inspector!");
        }
    }

    // Method to be called when the openOverlayButton is clicked
    public void OpenOverlay()
    {
        if (overlayPanel != null)
        {
            Debug.Log("Opening overlay panel.");
            overlayPanel.SetActive(true); // Show the overlay
        }
    }

    // Method to be called when the closeOverlayButton (inside the overlay) is clicked
    public void CloseOverlay()
    {
        if (overlayPanel != null)
        {
            Debug.Log("Closing overlay panel.");
            overlayPanel.SetActive(false); // Hide the overlay
        }
    }

    // Optional: Clean up listeners when the script is destroyed to prevent memory leaks
    void OnDestroy()
    {
        if (openOverlayButton != null)
        {
            openOverlayButton.onClick.RemoveListener(OpenOverlay);
        }
        if (closeOverlayButton != null)
        {
            closeOverlayButton.onClick.RemoveListener(CloseOverlay);
        }
    }
}