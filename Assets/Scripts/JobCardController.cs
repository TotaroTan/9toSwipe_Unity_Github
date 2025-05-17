using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections; // Required for IEnumerator

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

    [Header("Swipe Mechanics")]
    [SerializeField] private float swipeThresholdY = 200f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float swipeOutMoveSpeed = 3000f;

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
    private CanvasGroup cardCanvasGroup; // This is the correct variable name
    private Coroutine _currentSnapBackCoroutine;

    public bool isDraggingPublic => isDragging;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardCanvasGroup = GetComponent<CanvasGroup>();
        if (cardCanvasGroup == null) // CORRECTED: Was cardCanvasGMRp
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
    }

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
                Sprite logoSprite = Resources.Load<Sprite>("Logos/" + logoFileName);
                if (logoSprite != null)
                {
                    logoImage.sprite = logoSprite;
                    logoImage.color = Color.white;
                }
                else
                {
                    Debug.LogWarning($"Logo sprite not found in 'Resources/Logos/{logoFileName}' for domain '{data.domain}'. Assigning null to sprite.");
                    logoImage.sprite = null;
                    logoImage.color = new Color(1, 1, 1, 0);
                }
            }
            else
            {
                Debug.LogWarning($"Domain is empty for company '{data.company}', cannot derive logo filename. Assigning null to sprite.");
                logoImage.sprite = null;
                logoImage.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            Debug.LogWarning("LogoImage UI reference is not assigned in the JobCardController inspector for company: " + data.company);
        }

        if (companyNameText != null) companyNameText.text = data.company + (data.verified ? " ✓" : "");
        if (jobTitleText != null) jobTitleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        if (locationValueText != null) locationValueText.text = data.location;
        if (salaryValueText != null) salaryValueText.text = data.salary;
        if (experienceValueText != null) experienceValueText.text = data.experience;
        if (typeValueText != null) typeValueText.text = data.type;
        if (deadlineValueText != null) deadlineValueText.text = data.deadline;

        ResetCardVisualState();
        isSwipingOut = false;
        isDragging = false;
    }

    private void ResetCardVisualState()
    {
        if (rectTransform != null) targetY = initialStackPosition.y;

        if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
        if (applyButtonImage != null) applyButtonImage.color = originalApplyButtonColor;
        if (applyButtonCanvasGroup != null) applyButtonCanvasGroup.alpha = 1f;
    }

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
            applyButtonCanvasGroup.alpha = (deltaY > 0) ? 1f - Mathf.Clamp01(deltaY / swipeThresholdY) : 1f;
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
            duration = distanceToSnap / (returnSpeed * 100f);
        }
        duration = Mathf.Max(duration, 0.05f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isSwipingOut || isDragging)
            {
                _currentSnapBackCoroutine = null;
                yield break;
            }
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        if (!isSwipingOut && !isDragging)
        {
            rectTransform.anchoredPosition = endPos;
        }
        _currentSnapBackCoroutine = null;
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
        targetY = initialStackPosition.y + directionSign * (Screen.height * 1.2f);

        if (cardCanvasGroup != null)
        {
            cardCanvasGroup.interactable = false;
            cardCanvasGroup.blocksRaycasts = false;
        }
    }

    void OnApplyClicked()
    {
        if (isSwipingOut || (cardCanvasGroup != null && !cardCanvasGroup.interactable)) return;

        Debug.Log($"Apply clicked for: {currentJobData?.company} - {currentJobData?.title}");

        // --- ADD THIS SECTION ---
        if (currentJobData != null)
        {
            if (AppliedJobsManager.Instance != null)
            {
                AppliedJobsManager.Instance.AddAppliedJob(currentJobData);
            }
            else
            {
                Debug.LogError("[JobCardController] AppliedJobsManager.Instance is NULL when trying to add job!"); // THIS IS CRITICAL

            }
        }
        else
        {
            Debug.LogWarning("currentJobData is null on ApplyClicked.");
        }
        // --- END OF ADDED SECTION ---
 
 
        if (applyButtonImage != null)
        {
            applyButtonImage.color = Color.green;
        }
        TriggerProgrammaticSwipeUp();
    }

    private void TriggerProgrammaticSwipeUp()
    {
        if (isSwipingOut) return;
        Debug.Log($"Programmatic Swipe UP (Apply) : {currentJobData?.company}");
        StartSwipeOutVisuals(1);
    }

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