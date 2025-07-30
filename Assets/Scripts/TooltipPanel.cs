using UnityEngine;
using TMPro;

public class TooltipPanel : MonoBehaviour
{
    public TMP_Text infoTextUI;
    public TMP_Text titleTextUI;
    private TooltipManager manager;

    public void Setup(string title, string text, TooltipManager mgr)
    {
        if (titleTextUI != null) titleTextUI.text = title;
        infoTextUI.text = text;
        manager = mgr;
    }

    void Update()
    {
        // Guarda sempre la camera XR
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180f, 0); // per non essere al contrario
        }
    }

    public void Close()
    {
        manager.CloseTooltip();
    }
}
