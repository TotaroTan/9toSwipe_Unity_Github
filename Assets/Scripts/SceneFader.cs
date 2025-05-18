// SceneFader.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    public Image fadeImage;
    public float fadeDuration = 0.5f; // Duration of the fade in seconds

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persist
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (fadeImage == null)
        {
            Debug.LogError("SceneFader: FadeImage not assigned!");
            enabled = false;
            return;
        }
        // Start faded out
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
        fadeImage.gameObject.SetActive(false); // Keep it inactive until needed
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        yield return Fade(1f); // Fade to opaque

        // --- Before loading, ensure the Wishlist Canvas in the target scene will be active ---
        // This is tricky directly, so we rely on the target scene's setup.
        // The Wishlist Canvas in "ApplyAndWishlist" should be enabled by default
        // or enabled in an Awake() or Start() method of a script in that scene.

        SceneManager.LoadScene(sceneName);

        // SceneManager.LoadScene finishes loading *before* the next frame,
        // so Awake/Start of new scene scripts will run.
        // We'll start fading in from a script in the new scene, or from here if we can detect new scene load.
        // For simplicity, let's assume the new scene has a component that calls FadeInOnLoad()

        // The FadeIn part will be triggered by the new scene.
    }

    public void FadeInOnLoad()
    {
        // This should be called by a script in the newly loaded scene (e.g., in Start())
        fadeImage.gameObject.SetActive(true); // Ensure it's active if it was deactivated
        StartCoroutine(Fade(0f)); // Fade to transparent
    }


    private IEnumerator Fade(float targetAlpha)
    {
        float alpha = fadeImage.color.a;
        float timer = 0f;

        Color currentColor = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            alpha = Mathf.Lerp(currentColor.a, targetAlpha, timer / fadeDuration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

        if (targetAlpha == 0f)
        {
            fadeImage.gameObject.SetActive(false); // Deactivate when fully transparent
        }
    }
}