using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections; // Required for IEnumerator

public class HomeController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button registerButton;
    [SerializeField] private Button signInButton;
    [SerializeField] private RectTransform cardAreaPanel;

    [Header("Card Prefab")]
    [SerializeField] private GameObject jobCardPrefab;

    [Header("Card Stack Settings")]
    [SerializeField] private int maxVisibleCards = 3;
    [SerializeField] private float cardOffsetY = 20f;
    [SerializeField] private float cardScaleStep = 0.08f;

    private List<JobData> allJobs = new List<JobData>();
    private List<JobData> jobsToDisplay = new List<JobData>();
    private List<JobCardController> activeCardInstances = new List<JobCardController>();
    private Dictionary<Transform, Coroutine> _cardTransformCoroutines = new Dictionary<Transform, Coroutine>();

    void Start()
    {
        LoadJobData();
        SetupButtonListeners();
        SpawnInitialCards();
    }

    void LoadJobData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("companies");
        if (jsonFile != null)
        {
            JobListContainer jobListContainer = JsonUtility.FromJson<JobListContainer>(jsonFile.text);
            if (jobListContainer != null && jobListContainer.jobs != null)
            {
                allJobs = jobListContainer.jobs.ToList();
                System.Random rng = new System.Random();
                allJobs = allJobs.OrderBy(j => rng.Next()).ToList();
                jobsToDisplay = new List<JobData>(allJobs);
                Debug.Log($"Loaded and shuffled {allJobs.Count} jobs. Ready to display {jobsToDisplay.Count} jobs.");
            }
            else Debug.LogError("Failed to parse JSON or JSON structure is incorrect.");
        }
        else Debug.LogError("Could not load 'companies.json' from Resources folder!");
    }

    void SetupButtonListeners()
    {
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);
        if (signInButton != null) signInButton.onClick.AddListener(OnSignInClicked);
    }

    void SpawnInitialCards()
    {
        foreach (var cardController in activeCardInstances)
        {
            if (cardController != null)
            {
                StopTransformCoroutineForCard(cardController.transform);
                Destroy(cardController.gameObject);
            }
        }
        activeCardInstances.Clear();
        _cardTransformCoroutines.Clear();

        int cardsToSpawnCount = Mathf.Min(jobsToDisplay.Count, maxVisibleCards);
        for (int i = 0; i < cardsToSpawnCount; i++)
        {
            if (jobsToDisplay.Count > 0) SpawnNewCardAtBackOfStack();
        }
        UpdateCardStackVisuals();
    }

    void SpawnNewCardAtBackOfStack()
    {
        if (jobCardPrefab == null || cardAreaPanel == null || jobsToDisplay.Count == 0) return;

        JobData jobData = jobsToDisplay[0];
        jobsToDisplay.RemoveAt(0);

        GameObject cardInstanceGO = Instantiate(jobCardPrefab, cardAreaPanel);
        JobCardController cardController = cardInstanceGO.GetComponent<JobCardController>();

        if (cardController != null)
        {
            cardController.Initialize(this);
            cardController.Setup(jobData);
            activeCardInstances.Add(cardController);
        }
        else
        {
            Debug.LogError("JobCard_Prefab is missing JobCardController script!");
            Destroy(cardInstanceGO);
        }
    }

    public void CardSwiped(JobCardController swipedCardController)
    {
        StopTransformCoroutineForCard(swipedCardController.transform); // Use the base Transform as the key
        if (activeCardInstances.Contains(swipedCardController))
        {
            activeCardInstances.Remove(swipedCardController);
        }
        _cardTransformCoroutines.Remove(swipedCardController.transform); // Use the base Transform as the key

        if (jobsToDisplay.Count > 0 && activeCardInstances.Count < maxVisibleCards)
        {
            SpawnNewCardAtBackOfStack();
        }
        UpdateCardStackVisuals();

        if (activeCardInstances.Count == 0 && jobsToDisplay.Count == 0)
        {
            Debug.Log("All cards swiped and no more jobs to display!");
        }
    }

    void UpdateCardStackVisuals()
    {
        for (int i = 0; i < activeCardInstances.Count; i++)
        {
            JobCardController cardController = activeCardInstances[i];
            if (cardController == null || cardController.gameObject == null) continue;

            int stackVisualDepth = i;

            bool shouldBeActiveAndVisible = stackVisualDepth < maxVisibleCards;
            if (cardController.gameObject.activeSelf != shouldBeActiveAndVisible)
            {
                cardController.gameObject.SetActive(shouldBeActiveAndVisible);
            }

            if (!shouldBeActiveAndVisible)
            {
                StopTransformCoroutineForCard(cardController.transform);
                continue;
            }

            cardController.transform.SetSiblingIndex(cardAreaPanel.transform.childCount - 1 - stackVisualDepth);

            CanvasGroup cg = cardController.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                bool isTopAndReady = (stackVisualDepth == 0 && !cardController.IsSwipingOut());
                cg.interactable = isTopAndReady;
                cg.blocksRaycasts = isTopAndReady;
                cg.alpha = (stackVisualDepth == 0 || cardController.IsSwipingOut()) ? 1f : Mathf.Max(0.3f, 1f - (stackVisualDepth * 0.3f));
            }

            if (!cardController.IsSwipingOut() && !cardController.isDraggingPublic)
            {
                Vector2 targetAnchoredPos = new Vector2(0, -stackVisualDepth * cardOffsetY);
                float targetScale = Mathf.Max(0.1f, 1f - stackVisualDepth * cardScaleStep);

                cardController.SetRestingStackPosition(targetAnchoredPos);

                StopTransformCoroutineForCard(cardController.transform); // Use base Transform as key
                
                RectTransform cardRect = cardController.GetRectTransform(); // Get the RectTransform
                if (cardRect != null)
                {
                    Coroutine newCoroutine = StartCoroutine(SmoothCardTransform(cardRect, targetAnchoredPos, targetScale));
                    _cardTransformCoroutines[cardController.transform] = newCoroutine; // Store with base Transform as key
                }
                else
                {
                     Debug.LogError($"RectTransform on {cardController.name} is null. Cannot animate.");
                }
            }
        }
    }

    public void StopTransformCoroutineForCard(Transform cardKeyTransform) // Key is Transform
    {
        if (cardKeyTransform != null && _cardTransformCoroutines.ContainsKey(cardKeyTransform))
        {
            if (_cardTransformCoroutines[cardKeyTransform] != null)
            {
                StopCoroutine(_cardTransformCoroutines[cardKeyTransform]);
            }
            _cardTransformCoroutines.Remove(cardKeyTransform);
        }
    }

    // Parameter changed to RectTransform
    IEnumerator SmoothCardTransform(RectTransform cardRectTransform, Vector2 targetPosition, float targetScaleValue)
    {
        if (cardRectTransform == null) yield break;
        // Get JobCardController via the GameObject of the RectTransform
        JobCardController controller = cardRectTransform.gameObject.GetComponent<JobCardController>();

        float duration = 0.20f;
        float elapsed = 0f;
        Vector2 startPosition = cardRectTransform.anchoredPosition; // Use .anchoredPosition
        Vector3 startScale = cardRectTransform.localScale;
        Vector3 targetScaleVector = new Vector3(targetScaleValue, targetScaleValue, 1f);

        // Use cardRectTransform.transform as the key for the dictionary if removing from here
        Transform coroutineKey = cardRectTransform.transform; 

        while (elapsed < duration)
        {
            if (cardRectTransform == null) { _cardTransformCoroutines.Remove(coroutineKey); yield break; }
            if (controller != null && (controller.isDraggingPublic || controller.IsSwipingOut()))
            {
                _cardTransformCoroutines.Remove(coroutineKey);
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            cardRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t); // Use .anchoredPosition
            cardRectTransform.localScale = Vector3.Lerp(startScale, targetScaleVector, t);
            yield return null;
        }

        if (cardRectTransform != null)
        {
           if (controller == null || (!controller.isDraggingPublic && !controller.IsSwipingOut()))
           {
               cardRectTransform.anchoredPosition = targetPosition; // Use .anchoredPosition
               cardRectTransform.localScale = targetScaleVector;
           }
        }
        _cardTransformCoroutines.Remove(coroutineKey); // Remove when done or interrupted
    }

    void OnRegisterClicked() { Debug.Log("Register Button Clicked (Placeholder)"); }
    void OnSignInClicked() { Debug.Log("Sign In Button Clicked (Placeholder)"); }
}