// Inside JobCardController.cs (for the Home Scene)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class JobCardController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image logoImage;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI jobTitleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI locationValueText;
    [SerializeField] private TextMeshProUGUI salaryValueText;
    [SerializeField] private TextMeshProUGUI experienceValueText;
    [SerializeField] private TextMeshProUGUI typeValueText;
    [SerializeField] private TextMeshProUGUI deadlineValueText;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button starButton; // <<<--- ADD THIS LINE (for your Star/Wishlist button)
    [SerializeField] private Image verifiedIconImage; // <<<--- ADD THIS LINE (Reference for the Verified Tick Image)


    [Header("Swipe Mechanics")]
    [SerializeField] private float swipeThresholdY = 200f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float swipeOutMoveSpeed = 3000f;


    // ... (rest of your existing variables like currentJobData, rectTransform, etc.) ...
    public JobData currentJobData { get; private set; }
    private RectTransform rectTransform;
    private Vector2 initialStackPosition;
    private Vector2 dragStartLocalPosition;
    private bool isDragging = false;
    private bool isSwipingOut = false;
    private float targetY = 0;

    private HomeController homeController;
    private Image applyButtonImage;
    private CanvasGroup applyButtonCanvasGroup;
    private Color originalApplyButtonColor;
    private CanvasGroup cardCanvasGroup;
    private Coroutine _currentSnapBackCoroutine;

    // --- Optional: For changing star icon ---
    [Header("Star Icon Sprites (Optional)")]
    [SerializeField] private Sprite starIconFilled;   // Assign your "filled star" sprite in Inspector
    [SerializeField] private Sprite starIconOutline;  // Assign your "outline star" sprite in Inspector
    private Image starButtonImage;


    public bool isDraggingPublic => isDragging;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardCanvasGroup = GetComponent<CanvasGroup>();
        if (cardCanvasGroup == null)
        {
            cardCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyClicked);
            applyButtonImage = applyButton.GetComponent<Image>();
            if (applyButtonImage != null)
            {
                originalApplyButtonColor = applyButtonImage.color;
            }
            applyButtonCanvasGroup = applyButton.GetComponent<CanvasGroup>();
            if (applyButtonCanvasGroup == null)
            {
                applyButtonCanvasGroup = applyButton.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // --- ADDED SECTION for the Star Button ---
        if (starButton != null)
        {
            starButton.onClick.AddListener(OnStarOrWishlistClicked);
            starButtonImage = starButton.GetComponent<Image>(); // If you want to change its icon
        }
        // --- END OF ADDED SECTION ---
    }

    // ... (GetRectTransform, Initialize, SetRestingStackPosition methods) ...
    public RectTransform GetRectTransform()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        return rectTransform;
    }

    public void Initialize(HomeController controller)
    {
        homeController = controller;
        ResetCardVisualState();
        isSwipingOut = false;
        isDragging = false;
    }
     public void SetRestingStackPosition(Vector2 position)
    {
        initialStackPosition = position;
        if (!isDragging && !isSwipingOut)
        {
            targetY = initialStackPosition.y;
        }
    }


    public void Setup(JobData data)
    {
        currentJobData = data;

        // ... (your existing logoImage setup) ...
        if (logoImage != null)
        {
            string logoFileName = "";
            if (!string.IsNullOrEmpty(data.domain))
            {
                int dotIndex = data.domain.IndexOf('.');
                logoFileName = (dotIndex > 0) ? data.domain.Substring(0, dotIndex).ToLower() : data.domain.ToLower();
            }

            if (!string.IsNullOrEmpty(logoFileName))
            {
                Sprite logoSprite = Resources.Load<Sprite>("Logos/" + logoFileName); // Assumes logos are directly in Resources/Logos/
                if (logoSprite != null)
                {
                    logoImage.sprite = logoSprite;
                    logoImage.color = Color.white;
                }
                else
                {
                    // Fallback for logos inside Logos/square-logo/ if not found directly in Logos/
                    logoSprite = Resources.Load<Sprite>($"Logos/square-logo/{logoFileName}");
                    if (logoSprite != null)
                    {
                         logoImage.sprite = logoSprite;
                         logoImage.color = Color.white;
                    }
                    else
                    {
                        Debug.LogWarning($"Logo sprite not found in 'Resources/Logos/{logoFileName}' OR 'Resources/Logos/square-logo/{logoFileName}' for domain '{data.domain}'. Assigning null to sprite.");
                        logoImage.sprite = null;
                        logoImage.color = new Color(1, 1, 1, 0); // Make transparent
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Domain is empty for company '{data.company}', cannot derive logo filename. Assigning null to sprite.");
                logoImage.sprite = null;
                logoImage.color = new Color(1, 1, 1, 0); // Make transparent
            }
        }
        else
        {
            Debug.LogWarning("LogoImage UI reference is not assigned in the JobCardController inspector for company: " + data.company);
        }

        // --- MODIFIED THIS SECTION for Company Name and Verified Icon ---
        if (companyNameText != null)
        {
            companyNameText.text = data.company; // Display ONLY the company name, no character
        }

        // Control the visibility of the Verified Icon based on 'verified' property
        if (verifiedIconImage != null)
        {
            verifiedIconImage.gameObject.SetActive(data.verified); // Show/Hide the Image GameObject
        }
        else
        {
             Debug.LogWarning("VerifiedIconImage UI reference is not assigned in the JobCardController inspector for company: " + data.company);
        }
        // --- END OF MODIFIED SECTION ---


        if (jobTitleText != null) jobTitleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        if (locationValueText != null) locationValueText.text = data.location;
        if (salaryValueText != null) salaryValueText.text = data.salary;
        if (experienceValueText != null) experienceValueText.text = data.experience;
        if (typeValueText != null) typeValueText.text = data.type;
        if (deadlineValueText != null) deadlineValueText.text = data.deadline;


        ResetCardVisualState(); // This call will also update the star and verified icon states
        isSwipingOut = false;
        isDragging = false;
        // UpdateStarButtonVisual(); // Called inside ResetCardVisualState now
    }

    private void ResetCardVisualState()
    {
        // ... (your existing ResetCardVisualState code) ...
        if (rectTransform != null) targetY = initialStackPosition.y;

        // --- ADDED/MODIFIED THIS SECTION for Verified Icon visibility ---
        if (verifiedIconImage != null)
        {
             // Ensure icon visibility is correct when card resets or is reused
             verifiedIconImage.gameObject.SetActive(currentJobData != null && currentJobData.verified);
        }
         // --- END OF ADDED/MODIFIED SECTION ---


        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        if (applyButtonImage != null) applyButtonImage.color = originalApplyButtonColor;
        if (applyButtonCanvasGroup != null) applyButtonCanvasGroup.alpha = 1f;

        UpdateStarButtonVisual(); // Ensure star is correct on reset too
    }

    // ... (Update, OnBeginDrag, OnDrag, OnEndDrag, SmoothSnapBack, StartSwipeOutVisuals methods - UNCHANGED) ...
     void Update()
    {
        if (rectTransform == null) return;

        if (isSwipingOut)
        {
            Vector2 currentPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = Vector2.MoveTowards(currentPos, new Vector2(initialStackPosition.x, targetY), swipeOutMoveSpeed * Time.deltaTime);

            if (Mathf.Abs(rectTransform.anchoredPosition.y - targetY) < 10f)
            {
                gameObject.SetActive(false);
                if (homeController != null)
                {
                    homeController.CardSwiped(this);
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSwipingOut || (cardCanvasGroup != null && !cardCanvasGroup.interactable)) return;

        if (homeController != null) homeController.StopTransformCoroutineForCard(transform);
        if (_currentSnapBackCoroutine != null)
        {
            StopCoroutine(_currentSnapBackCoroutine);
            _currentSnapBackCoroutine = null;
        }

        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragStartLocalPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isSwipingOut || (cardCanvasGroup != null && !cardCanvasGroup.interactable)) return;

        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition);

        float deltaY = localPointerPosition.y - dragStartLocalPosition.y;
        rectTransform.anchoredPosition = new Vector2(initialStackPosition.x, initialStackPosition.y + deltaY);

        if (applyButtonCanvasGroup != null)
        {
            // Make apply button transparent as you drag *up*
             applyButtonCanvasGroup.alpha = Mathf.Clamp01(1f - (deltaY / swipeThresholdY));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || (cardCanvasGroup != null && !cardCanvasGroup.interactable)) return;
        isDragging = false;

        float currentDragDistanceY = rectTransform.anchoredPosition.y - initialStackPosition.y;

        if (Mathf.Abs(currentDragDistanceY) > swipeThresholdY)
        {
            StartSwipeOutVisuals(Mathf.Sign(currentDragDistanceY));
            Debug.Log($"Swiped {(currentDragDistanceY > 0 ? "UP" : "DOWN")} : {currentJobData?.company}");
        }
        else
        {
            // Reset alpha if not swiped far enough
            if (applyButtonCanvasGroup != null) applyButtonCanvasGroup.alpha = 1f;
            if (_currentSnapBackCoroutine != null) StopCoroutine(_currentSnapBackCoroutine);
            _currentSnapBackCoroutine = StartCoroutine(SmoothSnapBack());
        }
    }

    IEnumerator SmoothSnapBack()
    {
        if (isSwipingOut) { _currentSnapBackCoroutine = null; yield break; }

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(initialStackPosition.x, initialStackPosition.y);
        float distanceToSnap = Vector2.Distance(startPos, endPos);

        float duration = 0.2f;
        if (returnSpeed > 0.01f && distanceToSnap > 0.01f)
        {
            duration = distanceToSnap / (returnSpeed * 100f); // Scale duration based on distance and speed
        }
        duration = Mathf.Max(duration, 0.05f); // Minimum duration

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isSwipingOut || isDragging) // Interrupt if dragging or swiping out starts during snap
            {
                _currentSnapBackCoroutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        // Ensure it lands exactly on the target position
        if (!isSwipingOut && !isDragging)
        {
            rectTransform.anchoredPosition = endPos;
        }
        _currentSnapBackCoroutine = null; // Mark coroutine as finished
    }


    private void StartSwipeOutVisuals(float directionSign)
    {
        if (isSwipingOut) return;

        if (homeController != null) homeController.StopTransformCoroutineForCard(transform);
        if (_currentSnapBackCoroutine != null)
        {
            StopCoroutine(_currentSnapBackCoroutine);
            _currentSnapBackCoroutine = null;
        }

        isSwipingOut = true;
        // Calculate target way off screen based on swipe direction
        targetY = initialStackPosition.y + directionSign * (Screen.height * 1.2f);

        // Disable interaction while swiping out
        if (cardCanvasGroup != null)
        {
            cardCanvasGroup.interactable = false;
            cardCanvasGroup.blocksRaycasts = false;
        }
    }


    void OnApplyClicked()
    {
        // Prevent actions if already swiping out or interaction is blocked
        if (isSwipingOut || (cardCanvasGroup != null && !cardCanvasGroup.interactable)) return;

        Debug.Log($"[JobCardController] OnApplyClicked for: {currentJobData?.company} - {currentJobData?.title}");
        if (currentJobData != null)
        {
            if (AppliedJobsManager.Instance != null)
            {
                AppliedJobsManager.Instance.AddAppliedJob(currentJobData);
                 Debug.Log($"[JobCardController] Called AddAppliedJob for {currentJobData?.company}");
            }
            else
            {
                Debug.LogError("[JobCardController] AppliedJobsManager.Instance is NULL when trying to add job!");
            }
        }
        else
        {
            Debug.LogWarning("[JobCardController] currentJobData is null on ApplyClicked.");
        }

        // Visual feedback (optional)
        if (applyButtonImage != null)
        {
            applyButtonImage.color = Color.green;
        }
        // Trigger the swipe up after applying
        TriggerProgrammaticSwipeUp();
    }

    private void TriggerProgrammaticSwipeUp()
    {
        if (isSwipingOut) return;
        Debug.Log($"Programmatic Swipe UP (Apply) : {currentJobData?.company}");
        StartSwipeOutVisuals(1); // 1 means swipe up
    }

    // --- ADDED NEW METHOD ---
    public void OnStarOrWishlistClicked()
    {
        // Allow interaction unless strictly disabled, handles cases where card might not be at the very top
        if (isSwipingOut || (cardCanvasGroup != null && !cardCanvasGroup.interactable && !isDragging)) return;

        Debug.Log($"[JobCardController] Star/Wishlist clicked for: {currentJobData?.company} - {currentJobData?.title}");

        if (currentJobData != null)
        {
            if (WishlistManager.Instance != null)
            {
                if (WishlistManager.Instance.IsJobInWishlist(currentJobData))
                {
                    WishlistManager.Instance.RemoveFromWishlist(currentJobData);
                    Debug.Log($"[JobCardController] Removed {currentJobData.company} from wishlist.");
                }
                else
                {
                    WishlistManager.Instance.AddToWishlist(currentJobData);
                    Debug.Log($"[JobCardController] Added {currentJobData.company} to wishlist.");
                }
                UpdateStarButtonVisual(); // Update the star icon after action
            }
            else
            {
                Debug.LogError("[JobCardController] WishlistManager.Instance is null. Cannot manage wishlist.");
            }
        }
        else
        {
            Debug.LogWarning("[JobCardController] currentJobData is null on Star/Wishlist click.");
        }
    }

    // --- ADDED NEW METHOD (for visual feedback on star button) ---
    private void UpdateStarButtonVisual()
    {
        // Check if necessary references are assigned and current job data exists
        if (starButtonImage == null || starIconFilled == null || starIconOutline == null || WishlistManager.Instance == null || currentJobData == null)
        {
            // Optionally default to outline if starButtonImage exists but sprites or manager are missing
            if (starButtonImage != null && starIconOutline != null)
            {
                 starButtonImage.sprite = starIconOutline;
            }
             // Log a warning if critical components are missing, but only once if possible
             // Debug.LogWarning("Cannot update star button visual: Missing references or currentJobData.");
            return;
        }

        // Set the sprite based on whether the current job is in the wishlist
        if (WishlistManager.Instance.IsJobInWishlist(currentJobData))
        {
            starButtonImage.sprite = starIconFilled;
        }
        else
        {
            starButtonImage.sprite = starIconOutline;
        }
    }


    // ... (ForceSwipeUp, ForceSwipeDown, IsSwipingOut, GetCurrentJobData methods - UNCHANGED) ...
    public void ForceSwipeUp()
    {
        if (isSwipingOut) return;
        StartSwipeOutVisuals(1);
    }

    public void ForceSwipeDown()
    {
        if (isSwipingOut) return;
        StartSwipeOutVisuals(-1);
    }

    public bool IsSwipingOut() => isSwipingOut;
    public JobData GetCurrentJobData() => currentJobData;

}