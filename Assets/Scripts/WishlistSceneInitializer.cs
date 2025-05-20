// WishlistSceneInitializer.cs
using UnityEngine;

public class WishlistSceneInitializer : MonoBehaviour
{
    public GameObject wishlistCanvas; // Assign your Wishlist Canvas here in the Inspector

    void Awake()
    {
        // Ensure the Wishlist Canvas is active when this scene starts
        if (wishlistCanvas != null)
        {
            wishlistCanvas.SetActive(true);
        }
        else
        {
            // This error is important if you rely on this script to activate the canvas.
            // If your canvas is already active by default in the scene, this check is less critical
            // but still good for ensuring the reference is set if other logic might use it.
            Debug.LogError("WishlistSceneInitializer: Wishlist Canvas not assigned in the Inspector!");
        }
    }

    void Start()
    {
        // Tell the SceneFader to fade in
        if (SceneFader.Instance != null)
        {
            Debug.Log($"[{this.name} / {this.gameObject.scene.name}] Calling SceneFader.Instance.TriggerFadeIn()");
            SceneFader.Instance.TriggerFadeIn(); // <<< THIS IS THE CORRECTED LINE
        }
        else
        {
            // This would happen if SceneFader wasn't properly set up as a persistent singleton
            // or if this scene was loaded without SceneFader existing.
            Debug.LogWarning($"[{this.name} / {this.gameObject.scene.name}] SceneFader.Instance is null. Cannot trigger fade-in. Ensure SceneFader is in your initial scene and persists.");
        }
    }
}