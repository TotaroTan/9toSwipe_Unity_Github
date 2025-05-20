using UnityEngine;
using UnityEngine.UI; // Required for the Button type

public class LoginMainCanvasSwitcher : MonoBehaviour
{
    [Header("Main Canvases")]
    [Tooltip("Assign the 'User canvas' GameObject from your Hierarchy.")]
    public GameObject userCanvasRoot;

    [Tooltip("Assign the 'Business Canvas' GameObject from your Hierarchy.")]
    public GameObject businessCanvasRoot;

    [Header("Buttons on User Canvas")]
    [Tooltip("Assign the 'User toggle button' that is a child of 'User canvas'.")]
    public Button userCanvas_UserToggleButton;

    [Tooltip("Assign the 'Business toggle button' that is a child of 'User canvas'.")]
    public Button userCanvas_BusinessToggleButton;

    [Header("Buttons on Business Canvas")]
    [Tooltip("Assign the 'User toggle button' that is a child of 'Business Canvas'.")]
    public Button businessCanvas_UserToggleButton;

    [Tooltip("Assign the 'Business toggle button' that is a child of 'Business Canvas'.")]
    public Button businessCanvas_BusinessToggleButton;


    void Start()
    {
        // --- Null Checks for Essential Components ---
        if (userCanvasRoot == null)
        {
            Debug.LogError("LoginMainCanvasSwitcher: 'User Canvas Root' is not assigned in the Inspector!");
            enabled = false; // Disable script to prevent further errors
            return;
        }
        if (businessCanvasRoot == null)
        {
            Debug.LogError("LoginMainCanvasSwitcher: 'Business Canvas Root' is not assigned in the Inspector!");
            enabled = false;
            return;
        }

        // Check buttons (optional, but good for debugging setup issues)
        if (userCanvas_UserToggleButton == null) Debug.LogWarning("LoginMainCanvasSwitcher: 'User Canvas_User Toggle Button' not assigned. Interactability will not be managed for this button.");
        if (userCanvas_BusinessToggleButton == null) Debug.LogWarning("LoginMainCanvasSwitcher: 'User Canvas_Business Toggle Button' not assigned. Interactability will not be managed for this button.");
        if (businessCanvas_UserToggleButton == null) Debug.LogWarning("LoginMainCanvasSwitcher: 'Business Canvas_User Toggle Button' not assigned. Interactability will not be managed for this button.");
        if (businessCanvas_BusinessToggleButton == null) Debug.LogWarning("LoginMainCanvasSwitcher: 'Business Canvas_Business Toggle Button' not assigned. Interactability will not be managed for this button.");


        // --- Programmatically add listeners OR rely on Inspector OnClick() ---
        // Option 1: Programmatic (if you prefer to set it up in code)
        // Ensure you DON'T also set it up in the Inspector's OnClick() if you use this, or it will fire twice.

        // Listeners for buttons on User Canvas
        if (userCanvas_UserToggleButton != null)
            userCanvas_UserToggleButton.onClick.AddListener(ActivateUserCanvasMode);
        if (userCanvas_BusinessToggleButton != null)
            userCanvas_BusinessToggleButton.onClick.AddListener(ActivateBusinessCanvasMode);

        // Listeners for buttons on Business Canvas
        if (businessCanvas_UserToggleButton != null)
            businessCanvas_UserToggleButton.onClick.AddListener(ActivateUserCanvasMode);
        if (businessCanvas_BusinessToggleButton != null)
            businessCanvas_BusinessToggleButton.onClick.AddListener(ActivateBusinessCanvasMode);

        // Set initial state (e.g., User canvas active)
        ActivateUserCanvasMode();
    }

    /// <summary>
    /// Activates the User Canvas and deactivates the Business Canvas.
    /// Manages button interactability.
    /// </summary>
    public void ActivateUserCanvasMode()
    {
        if (userCanvasRoot != null) userCanvasRoot.SetActive(true);
        if (businessCanvasRoot != null) businessCanvasRoot.SetActive(false);

        // Manage interactability of buttons on the now visible User Canvas
        if (userCanvas_UserToggleButton != null) userCanvas_UserToggleButton.interactable = false; // Can't switch to self
        if (userCanvas_BusinessToggleButton != null) userCanvas_BusinessToggleButton.interactable = true;

        // Debug.Log("User Canvas Mode Activated");
    }

    /// <summary>
    /// Activates the Business Canvas and deactivates the User Canvas.
    /// Manages button interactability.
    /// </summary>
    public void ActivateBusinessCanvasMode()
    {
        if (userCanvasRoot != null) userCanvasRoot.SetActive(false);
        if (businessCanvasRoot != null) businessCanvasRoot.SetActive(true);

        // Manage interactability of buttons on the now visible Business Canvas
        if (businessCanvas_UserToggleButton != null) businessCanvas_UserToggleButton.interactable = true;
        if (businessCanvas_BusinessToggleButton != null) businessCanvas_BusinessToggleButton.interactable = false; // Can't switch to self

        // Debug.Log("Business Canvas Mode Activated");
    }

    // Optional: If you added listeners programmatically, it's good practice to remove them
    // void OnDestroy()
    // {
    //     if (userCanvas_UserToggleButton != null) userCanvas_UserToggleButton.onClick.RemoveListener(ActivateUserCanvasMode);
    //     if (userCanvas_BusinessToggleButton != null) userCanvas_BusinessToggleButton.onClick.RemoveListener(ActivateBusinessCanvasMode);
    //     if (businessCanvas_UserToggleButton != null) businessCanvas_UserToggleButton.onClick.RemoveListener(ActivateUserCanvasMode);
    //     if (businessCanvas_BusinessToggleButton != null) businessCanvas_BusinessToggleButton.onClick.RemoveListener(ActivateBusinessCanvasMode);
    // }
}