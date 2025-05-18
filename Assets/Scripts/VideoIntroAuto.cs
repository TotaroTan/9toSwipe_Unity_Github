using UnityEngine;
using UnityEngine.Video;

public class VideoIntroOverlay : MonoBehaviour
{
    public VideoPlayer videoPlayer;       // The VideoPlayer on the overlay
    public GameObject videoOverlayPanel;  // The overlay panel itself
    public GameObject[] uiPanelsToHide;   // Panels to hide while video plays (optional)

    private static bool hasPlayedThisSession = false; // Resets on app restart

    void Start()
    {
        if (hasPlayedThisSession)
        {
            // Skip video
            if (videoOverlayPanel != null)
                videoOverlayPanel.SetActive(false);

            if (uiPanelsToHide != null)
            {
                foreach (var panel in uiPanelsToHide)
                {
                    if (panel != null)
                        panel.SetActive(true);
                }
            }

            return;
        }

        // First time in this session
        if (videoOverlayPanel != null)
            videoOverlayPanel.SetActive(false);

        if (uiPanelsToHide != null)
        {
            foreach (var panel in uiPanelsToHide)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        if (videoOverlayPanel != null)
            videoOverlayPanel.SetActive(true);

        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (videoOverlayPanel != null)
            videoOverlayPanel.SetActive(false);

        if (uiPanelsToHide != null)
        {
            foreach (var panel in uiPanelsToHide)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }

        // Mark as played for this session only
        hasPlayedThisSession = true;
    }
}
