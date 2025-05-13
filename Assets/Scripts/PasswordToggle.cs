using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure TextMeshPro is imported

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))] // Expects an Image component for the icon
public class PasswordToggle : MonoBehaviour
{
    [Tooltip("The Input Field this button controls.")]
    [SerializeField] private TMP_InputField passwordInputField;

    [Header("Eye Icon Sprites")]
    [SerializeField] private Sprite eyeVisibleSprite; // Assign Visible Eye Sprite
    [SerializeField] private Sprite eyeHiddenSprite; // Assign Hidden/Closed Eye Sprite

    private Button toggleButton;
    private Image buttonImage;
    private bool isPasswordVisible = false;

    void Awake()
    {
        toggleButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        if (passwordInputField == null)
        {
            Debug.LogError($"PasswordToggle on {gameObject.name}: Password Input Field not assigned!", this);
            return;
        }
        if (eyeVisibleSprite == null || eyeHiddenSprite == null)
        {
             Debug.LogError($"PasswordToggle on {gameObject.name}: Eye sprites not assigned!", this);
            return;
        }

        // Ensure input field starts as password
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        isPasswordVisible = false;
        UpdateIcon(); // Set initial icon

        toggleButton.onClick.AddListener(ToggleVisibility);
    }

    void ToggleVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
        }

        // IMPORTANT: Need to force update the input field visuals after changing content type
        passwordInputField.ForceLabelUpdate();
         // Re-activate to ensure caret and visuals refresh correctly (sometimes needed)
         passwordInputField.DeactivateInputField();
         passwordInputField.ActivateInputField();

        UpdateIcon();
    }

    void UpdateIcon()
    {
         if (buttonImage != null)
         {
             buttonImage.sprite = isPasswordVisible ? eyeVisibleSprite : eyeHiddenSprite;
         }
    }

     // Optional: Remove listener on destroy
    void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleVisibility);
        }
    }
}