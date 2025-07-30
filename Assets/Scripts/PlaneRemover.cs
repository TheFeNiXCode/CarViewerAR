using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneRemover : MonoBehaviour
{

    public ARPlaneManager planeManager;
    public ARSession arSession;

    public void DisableAllPlanes()
    {
        // Disattiva il rilevamento di nuovi piani
        planeManager.enabled = false;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }


    // Resetta completamente la sessione AR
    public void ResetARSession()
    {
        // Disattiva il rilevamento piani
        planeManager.enabled = false;

        // Reset della sessione AR
        if (arSession != null)
        {
            arSession.Reset();
        }
    }
}
