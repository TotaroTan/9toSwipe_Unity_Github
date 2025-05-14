using UnityEngine;

public class CameraStatusChecker : MonoBehaviour
{
    public Camera cameraToCheck; // Drag your "Main Camera" here in the Inspector

    void Update()
    {
        if (cameraToCheck == null)
        {
            Debug.LogError("CAMERA STATUS: cameraToCheck is NULL!");
        }
        else
        {
            if (!cameraToCheck.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("CAMERA STATUS: cameraToCheck GameObject is INACTIVE in Hierarchy!");
            }
            if (!cameraToCheck.enabled)
            {
                Debug.LogWarning("CAMERA STATUS: cameraToCheck Component is DISABLED!");
            }

            // Optional: Check if it's still the main camera for the canvas
            Canvas canvas = FindObjectOfType<Canvas>(); // Find your main UI canvas
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    Debug.LogError("CANVAS STATUS: Canvas worldCamera is NULL!");
                }
                else if (canvas.worldCamera != cameraToCheck)
                {
                    Debug.LogWarning("CANVAS STATUS: Canvas worldCamera is NOT our cameraToCheck!");
                }
            }
        }
    }
}