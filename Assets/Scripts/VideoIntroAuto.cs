using UnityEngine;
using UnityEngine.Video;

public class VideoIntroOverlay : MonoBehaviour
{
    public VideoPlayer videoPlayer;       // The VideoPlayer on the overlay
    public GameObject videoOverlayPanel;  // The overlay panel itself
    public GameObject[] uiPanelsToHide;   // Panels to hide while video plays (optional)

    void Start()
    {
        // Start with video overlay panel hidden
        if (videoOverlayPanel != null)
            videoOverlayPanel.SetActive(false);

        // Hide all UI panels underneath, if any
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
        // Activate the overlay panel when video is ready
        if (videoOverlayPanel != null)
            videoOverlayPanel.SetActive(true);

        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Hide the overlay panel when video finishes
        if (videoOverlayPanel != null)
            videoOverlayPanel.SetActive(false);

        // Show all UI panels again
        if (uiPanelsToHide != null)
        {
            foreach (var panel in uiPanelsToHide)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }
    }
}