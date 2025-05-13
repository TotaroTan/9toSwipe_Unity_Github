using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: Scene name cannot be empty!");
            return;
        }
        Debug.Log($"SceneLoader: Loading scene '{sceneName}'...");
        SceneManager.LoadScene(sceneName);
    }

    // Optional: Add methods for async loading if needed later
}