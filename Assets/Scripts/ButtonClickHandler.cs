using UnityEngine;
using UnityEngine.UI; // Required for Button

[RequireComponent(typeof(Button))]
public class ButtonClickHandler : MonoBehaviour
{
    [Tooltip("The name of the scene to load when this button is clicked.")]
    [SerializeField] private string targetSceneName;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogError($"Button component not found on {gameObject.name}");
        }
    }

    void HandleClick()
    {
         if (!string.IsNullOrEmpty(targetSceneName))
         {
              Debug.Log($"{gameObject.name} clicked, loading scene: {targetSceneName}");
              SceneLoader.LoadScene(targetSceneName);
         }
         else
         {
              Debug.LogWarning($"ButtonClickHandler on {gameObject.name} has no targetSceneName set.");
              // Optionally, add specific logic here if the button does something else
              if (gameObject.name.Contains("Avatar")) {
                   // Example: specific action for avatar
                   Debug.Log("Avatar button specific action (if any besides scene load)");
              }
         }
    }

    // Optional: Remove listener on destroy
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}