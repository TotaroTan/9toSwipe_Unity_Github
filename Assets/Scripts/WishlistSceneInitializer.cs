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
            Debug.LogError("WishlistSceneInitializer: Wishlist Canvas not assigned!");
        }
    }

    void Start()
    {
        // Tell the SceneFader to fade in
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeInOnLoad();
        }
    }
}