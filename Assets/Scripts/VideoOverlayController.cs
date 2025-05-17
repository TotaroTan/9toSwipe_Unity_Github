using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoOverlayController : MonoBehaviour
{
    public GameObject videoScreen;         // UI container (can be a panel)
    public RawImage videoImage;            // UI RawImage displaying the RenderTexture
    public VideoPlayer videoPlayer;        // VideoPlayer component
    public RenderTexture renderTexture;    // The target RenderTexture

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;

        // Clear screen and RenderTexture at start
        ClearRenderTexture();
        videoImage.texture = renderTexture;
        videoScreen.SetActive(false);
    }

    public void PlayVideo()
    {
        StartCoroutine(PlayAfterPrepare());
    }

    private System.Collections.IEnumerator PlayAfterPrepare()
    {
        // Hide screen and clear texture first
        videoScreen.SetActive(false);
        ClearRenderTexture();

        // Prepare the video
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoScreen.SetActive(true);
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoPlayer.Stop();
        videoScreen.SetActive(false);
        ClearRenderTexture();
    }

    void ClearRenderTexture()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = currentRT;
    }
}
