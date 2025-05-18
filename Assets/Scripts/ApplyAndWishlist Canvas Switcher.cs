using UnityEngine;
using UnityEngine.UI; // Required for Button

public class TabbedCanvasManager : MonoBehaviour
{
    [Header("Canvases to Toggle")]
    [SerializeField] private GameObject wishlistCanvasGameObject;
    [SerializeField] private GameObject applyCanvasGameObject;

    [Header("Buttons on Wishlist Canvas")]
    [SerializeField] private Button wishlistCanvas_ShowApplyButton;   // The "Applied" button on the Wishlist Canvas
    [SerializeField] private Button wishlistCanvas_ShowWishlistButton; // The "Wishlist" button on the Wishlist Canvas

    [Header("Buttons on Apply Canvas")]
    [SerializeField] private Button applyCanvas_ShowApplyButton;     // The "Applied" button on the Apply Canvas
    [SerializeField] private Button applyCanvas_ShowWishlistButton;  // The "Wishlist" button on the Apply Canvas

    void Start()
    {
        // --- Initial State: Show one canvas, hide the other ---
        // Decide which canvas should be visible by default, e.g., Apply Canvas
        ShowApplyCanvas();
        // If you want Wishlist to be default: ShowWishlistCanvas();

        // --- Assign Button Listeners ---
        if (wishlistCanvas_ShowApplyButton != null)
            wishlistCanvas_ShowApplyButton.onClick.AddListener(ShowApplyCanvas);
        else Debug.LogError("wishlistCanvas_ShowApplyButton not assigned in TabbedCanvasManager!");

        if (wishlistCanvas_ShowWishlistButton != null)
        {
            wishlistCanvas_ShowWishlistButton.onClick.AddListener(ShowWishlistCanvas);
            // Optionally disable the button that shows the current canvas
            // wishlistCanvas_ShowWishlistButton.interactable = false; // Example if Wishlist is default
        }
        else Debug.LogError("wishlistCanvas_ShowWishlistButton not assigned in TabbedCanvasManager!");


        if (applyCanvas_ShowApplyButton != null)
        {
            applyCanvas_ShowApplyButton.onClick.AddListener(ShowApplyCanvas);
            // Optionally disable the button that shows the current canvas
            // applyCanvas_ShowApplyButton.interactable = false; // Example if Apply is default
        }
        else Debug.LogError("applyCanvas_ShowApplyButton not assigned in TabbedCanvasManager!");

        if (applyCanvas_ShowWishlistButton != null)
            applyCanvas_ShowWishlistButton.onClick.AddListener(ShowWishlistCanvas);
        else Debug.LogError("applyCanvas_ShowWishlistButton not assigned in TabbedCanvasManager!");

    }

    public void ShowWishlistCanvas()
    {
        if (wishlistCanvasGameObject != null) wishlistCanvasGameObject.SetActive(true);
        if (applyCanvasGameObject != null) applyCanvasGameObject.SetActive(false);

        // Update button interactability
        if (wishlistCanvas_ShowWishlistButton != null) wishlistCanvas_ShowWishlistButton.interactable = false;
        if (wishlistCanvas_ShowApplyButton != null) wishlistCanvas_ShowApplyButton.interactable = true;

        if (applyCanvas_ShowWishlistButton != null) applyCanvas_ShowWishlistButton.interactable = false; // It's showing wishlist
        if (applyCanvas_ShowApplyButton != null) applyCanvas_ShowApplyButton.interactable = true;

        Debug.Log("Switched to Wishlist Canvas");
    }

    public void ShowApplyCanvas()
    {
        if (wishlistCanvasGameObject != null) wishlistCanvasGameObject.SetActive(false);
        if (applyCanvasGameObject != null) applyCanvasGameObject.SetActive(true);

        // Update button interactability
        if (wishlistCanvas_ShowWishlistButton != null) wishlistCanvas_ShowWishlistButton.interactable = true;
        if (wishlistCanvas_ShowApplyButton != null) wishlistCanvas_ShowApplyButton.interactable = false; // It's showing apply

        if (applyCanvas_ShowWishlistButton != null) applyCanvas_ShowWishlistButton.interactable = true;
        if (applyCanvas_ShowApplyButton != null) applyCanvas_ShowApplyButton.interactable = false;

        Debug.Log("Switched to Apply Canvas");
    }
}