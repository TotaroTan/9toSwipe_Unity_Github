using UnityEngine;

public class ShowPanel : MonoBehaviour
{
    public GameObject targetPanel; // Assign the panel you want to show in the Inspector

    public void ShowOnlyThisPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }
}