// SceneFader.cs (Asynchronous Loading Version)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Optional: For progress text

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("Fade Elements")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Loading Screen Elements (Optional)")]
    [SerializeField] private GameObject loadingScreenElementsContainer; // Parent for progress bar, text
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    private AsyncOperation sceneLoadOperation;
    private string sceneToLoadName; // To store the name of the scene being loaded

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupInitialState(); // Setup after instance is confirmed
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return; // Return early if destroying self
        }
    }

    private void SetupInitialState()
    {
        if (fadeImage == null)
        {
            Debug.LogError("[SceneFader] FadeImage not assigned in the Inspector!");
            enabled = false; // Disable script if essential component is missing
            return;
        }
        // Ensure it starts transparent and inactive
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);

        if (loadingScreenElementsContainer != null)
        {
            loadingScreenElementsContainer.SetActive(false);
        }
        if (progressBar != null) progressBar.value = 0;
        if (progressText != null) progressText.text = "";
    }

    /// <summary>
    /// Starts fading out, then loads the specified scene asynchronously,
    /// then calls TriggerFadeIn from the new scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadSceneAsyncWithFade(string sceneName) // <<< THIS IS THE METHOD NEEDED
    {
        if (sceneLoadOperation != null && !sceneLoadOperation.isDone && sceneLoadOperation.progress < 0.9f) // Check if already loading and not ready to activate
        {
            Debug.LogWarning($"[SceneFader] Already attempting to load scene '{sceneToLoadName}'. New request for '{sceneName}' ignored.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneFader] Scene name to load is null or empty!");
            return;
        }

        this.sceneToLoadName = sceneName;
        StartCoroutine(FadeOutAndLoadSceneAsyncCoroutine(sceneName));
    }

    private IEnumerator FadeOutAndLoadSceneAsyncCoroutine(string sceneName)
    {
        Debug.Log($"[SceneFader] [{Time.frameCount}] FadeOutAndLoadSceneAsyncCoroutine START for scene: {sceneName}");

        fadeImage.gameObject.SetActive(true);
        SetFadeImageAlpha(0f); // Start transparent for the fade-out

        if (loadingScreenElementsContainer != null)
        {
            loadingScreenElementsContainer.SetActive(true);
            if (progressBar != null) progressBar.value = 0;
            if (progressText != null) progressText.text = "Loading... 0%";
        }

        Debug.Log($"[SceneFader] [{Time.frameCount}] Fading to Opaque...");
        yield return FadeInternal(1f, fadeDuration); // Fade to fully opaque

        Debug.Log($"[SceneFader] [{Time.frameCount}] Starting async load for: {sceneName}");
        sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (sceneLoadOperation == null)
        {
            Debug.LogError($"[SceneFader] Could not start loading scene '{sceneName}'. Is it in Build Settings?");
            if (loadingScreenElementsContainer != null) loadingScreenElementsContainer.SetActive(false);
            yield return FadeInternal(0f, fadeDuration); // Fade back in current scene
            yield break;
        }
        sceneLoadOperation.allowSceneActivation = false;

        while (!sceneLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(sceneLoadOperation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

            if (sceneLoadOperation.progress >= 0.9f)
            {
                Debug.Log($"[SceneFader] [{Time.frameCount}] Scene '{sceneName}' is ready to activate. FadeImage Alpha: {fadeImage.color.a}");
                SetFadeImageAlpha(1f); // Ensure fully opaque
                fadeImage.gameObject.SetActive(true);

                Debug.Log($"[SceneFader] [{Time.frameCount}] Activating scene: {sceneName}");
                sceneLoadOperation.allowSceneActivation = true;
                // New scene will activate. SceneFader persists.
                // TriggerFadeIn() will be called from the new scene.
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Call this from the Start() method of a script in the newly loaded scene.
    /// Renamed from FadeInOnLoad to be more explicit for the async flow.
    /// </summary>
    public void TriggerFadeIn()
    {
        Debug.Log($"[SceneFader] [{Time.frameCount}] TriggerFadeIn CALLED for scene: {sceneToLoadName}. Current FadeImage Alpha: {(fadeImage != null ? fadeImage.color.a.ToString("F2") : "N/A")}");

        if (fadeImage == null)
        {
            Debug.LogError("[SceneFader] FadeImage is null in TriggerFadeIn. Cannot fade.");
            return;
        }

        if (loadingScreenElementsContainer != null && loadingScreenElementsContainer != fadeImage.gameObject)
        {
            loadingScreenElementsContainer.SetActive(false);
        }
        if (progressBar != null) progressBar.value = 0;
        if (progressText != null) progressText.text = "";

        fadeImage.gameObject.SetActive(true);
        SetFadeImageAlpha(1f); // Ensure fully opaque before starting fade-in

        StartCoroutine(FadeInternal(0f, fadeDuration));
    }

    private void SetFadeImageAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    // Renamed Fade to FadeInternal to avoid conflict if you were to add a public Fade method
    private IEnumerator FadeInternal(float targetAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        if (!fadeImage.gameObject.activeInHierarchy && (targetAlpha > 0 || (targetAlpha == 0 && fadeImage.color.a > 0)))
        {
             // Activate if fading in, or if fading out from a visible state
            fadeImage.gameObject.SetActive(true);
        }


        if (duration <= 0)
        {
            SetFadeImageAlpha(targetAlpha);
        }
        else
        {
            float startAlpha = fadeImage.color.a;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                SetFadeImageAlpha(newAlpha);
                yield return null;
            }
            SetFadeImageAlpha(targetAlpha);
        }

        if (targetAlpha == 0f && fadeImage.gameObject.activeSelf)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}