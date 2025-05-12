using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen Panels")]
    public GameObject signIn1Panel;
    public GameObject registerPanel;
    public GameObject signIn2Panel;
    public GameObject homePanel; // For after login

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: if you plan to have multiple scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Start with the SignIn1 screen
        ShowSignIn1Screen();
    }

    private void HideAllScreens()
    {
        signIn1Panel.SetActive(false);
        registerPanel.SetActive(false);
        signIn2Panel.SetActive(false);
        if (homePanel) homePanel.SetActive(false);
    }

    public void ShowSignIn1Screen()
    {
        HideAllScreens();
        signIn1Panel.SetActive(true);
    }

    public void ShowRegisterScreen()
    {
        HideAllScreens();
        registerPanel.SetActive(true);
    }

    public void ShowSignIn2Screen()
    {
        HideAllScreens();
        signIn2Panel.SetActive(true);
    }

    public void ShowHomeScreen()
    {
        HideAllScreens();
        if (homePanel) homePanel.SetActive(true);
        else Debug.Log("HomePanel not assigned or created.");
    }
}