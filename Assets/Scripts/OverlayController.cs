using UnityEngine;

public class OverlayController : MonoBehaviour
{
    public GameObject overlayPanel;

    public void ShowOverlay()
    {
        StartCoroutine(ShowAndHideOverlay());
    }

    private System.Collections.IEnumerator ShowAndHideOverlay()
    {
        overlayPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        overlayPanel.SetActive(false);
    }
}
