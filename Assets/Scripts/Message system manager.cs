using UnityEngine;
using UnityEngine.UI; // REQUIRED for Button type

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject allMessagesCanvas;
    public GameObject unreadMessagesCanvas;
    public GameObject requestMessagesCanvas;

    // --- Button References ---
    // We'll group them by the canvas they reside in for better organization in the Inspector.

    [System.Serializable] // This makes the class show up in the Inspector
    public class ButtonSet
    {
        public Button allButton;
        public Button unreadButton;
        public Button requestButton;
    }

    [Header("Buttons within 'All Messages Canvas'")]
    public ButtonSet buttonsInAllMessagesCanvas;

    [Header("Buttons within 'Unread Messages Canvas'")]
    public ButtonSet buttonsInUnreadMessagesCanvas;

    [Header("Buttons within 'Request Messages Canvas'")]
    public ButtonSet buttonsInRequestMessagesCanvas;


    public enum ActiveCanvas
    {
        AllMessages,
        UnreadMessages,
        RequestMessages
    }

    void Awake() // Changed to Awake to ensure listeners are set up before other Start methods might try to use them
    {
        // --- Canvas Null Checks (Important!) ---
        if (allMessagesCanvas == null) Debug.LogError("All Messages Canvas is not assigned to CanvasSwitcher!");
        if (unreadMessagesCanvas == null) Debug.LogError("Unread Messages Canvas is not assigned to CanvasSwitcher!");
        if (requestMessagesCanvas == null) Debug.LogError("Request Messages Canvas is not assigned to CanvasSwitcher!");
        if (allMessagesCanvas == null || unreadMessagesCanvas == null || requestMessagesCanvas == null)
        {
            Debug.LogError("One or more PARENT CANVASES are not assigned. Disabling CanvasSwitcher.");
            enabled = false;
            return;
        }

        // --- Assign Button Listeners ---
        AssignListenersForSet(buttonsInAllMessagesCanvas, "All Messages Canvas");
        AssignListenersForSet(buttonsInUnreadMessagesCanvas, "Unread Messages Canvas");
        AssignListenersForSet(buttonsInRequestMessagesCanvas, "Request Messages Canvas");

        // Set an initial active canvas
        ShowAllMessages();
    }

    void AssignListenersForSet(ButtonSet buttonSet, string setNameForLogging)
    {
        if (buttonSet.allButton != null)
        {
            buttonSet.allButton.onClick.AddListener(ShowAllMessages);
        }
        else
        {
            Debug.LogWarning($"'All Button' in '{setNameForLogging}' set is not assigned in the Inspector.");
        }

        if (buttonSet.unreadButton != null)
        {
            buttonSet.unreadButton.onClick.AddListener(ShowUnreadMessages);
        }
        else
        {
            Debug.LogWarning($"'Unread Button' in '{setNameForLogging}' set is not assigned in the Inspector.");
        }

        if (buttonSet.requestButton != null)
        {
            buttonSet.requestButton.onClick.AddListener(ShowRequestMessages);
        }
        else
        {
            Debug.LogWarning($"'Request Button' in '{setNameForLogging}' set is not assigned in the Inspector.");
        }
    }


    // --- Public Methods to be called by UI Buttons (or internally now) ---

    public void ShowAllMessages()
    {
        SetActiveCanvas(ActiveCanvas.AllMessages);
        Debug.Log("Switched to All Messages Canvas");
    }

    public void ShowUnreadMessages()
    {
        SetActiveCanvas(ActiveCanvas.UnreadMessages);
        Debug.Log("Switched to Unread Messages Canvas");
    }

    public void ShowRequestMessages()
    {
        SetActiveCanvas(ActiveCanvas.RequestMessages);
        Debug.Log("Switched to Request Messages Canvas");
    }

    // --- Private helper method to handle the actual activation/deactivation ---

    private void SetActiveCanvas(ActiveCanvas canvasToActivate)
    {
        if (allMessagesCanvas != null) allMessagesCanvas.SetActive(false);
        if (unreadMessagesCanvas != null) unreadMessagesCanvas.SetActive(false);
        if (requestMessagesCanvas != null) requestMessagesCanvas.SetActive(false);

        switch (canvasToActivate)
        {
            case ActiveCanvas.AllMessages:
                if (allMessagesCanvas != null) allMessagesCanvas.SetActive(true);
                break;
            case ActiveCanvas.UnreadMessages:
                if (unreadMessagesCanvas != null) unreadMessagesCanvas.SetActive(true);
                break;
            case ActiveCanvas.RequestMessages:
                if (requestMessagesCanvas != null) requestMessagesCanvas.SetActive(true);
                break;
        }
    }
}