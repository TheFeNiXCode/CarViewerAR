using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipManager : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera; // AR Camera (XR Origin)
    public GameObject tooltipPanelPrefab;
    public SpawnManager spawnManager;

    [Header("Settings")]
    [SerializeField] private float verticalOffset = 0.2f; // distanza verticale del pannello sopra il tooltip

    private GameObject tooltipPanelInstance;
    private TooltipPanel tooltipPanelScript;
    private TooltipPoint activeTooltip;

    void Update()
    {
        // Gestione input touch o click
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TrySelect(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            TrySelect(Input.mousePosition);
        }
    }

    void TrySelect(Vector2 screenPosition)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            // Procedi solo se il tag è "Info", altrimenti ignora il click
            if (!hit.collider.gameObject.CompareTag("Info"))
            {
                return;
            }

            TooltipPoint tp = hit.collider.GetComponent<TooltipPoint>();
            if (tp != null)
            {
                if (tp == activeTooltip)
                {
                    CloseTooltip();
                }
                else
                {
                    ShowTooltip(tp);
                }
            }
            else
            {
                CloseTooltip();
            }
        }
        else
        {
            if (tooltipPanelInstance != null)
                CloseTooltip();
        }
    }

    void ShowTooltip(TooltipPoint tp)
    {
        // Se non esiste ancora il pannello, lo creiamo ora come figlio del parent del target
        if (tooltipPanelInstance == null)
        {
            Transform parent = tp.transform.parent != null ? tp.transform.parent : tp.transform;
            tooltipPanelInstance = Instantiate(tooltipPanelPrefab, parent);
            tooltipPanelScript = tooltipPanelInstance.GetComponent<TooltipPanel>();
        }

        // Riattiva eventuale tooltip precedente
        if (activeTooltip != null)
            activeTooltip.ResumeAnimation();

        activeTooltip = tp;
        activeTooltip.PauseAnimation();

        float offsetV = verticalOffset * spawnManager.carScale;

        // Posiziona pannello
        Vector3 spawnPos = tp.transform.position + Vector3.up * offsetV;
        tooltipPanelInstance.transform.position = spawnPos;

        // Aggiorna testo
        tooltipPanelScript.Setup(tp.title, tp.infoText, this);

        // Mostra pannello
        tooltipPanelInstance.SetActive(true);
    }

    public void CloseTooltip()
    {
        if (tooltipPanelInstance == null)
            return;

        if (activeTooltip != null)
        {
            activeTooltip.ResumeAnimation();
            activeTooltip = null;
        }

        tooltipPanelInstance.SetActive(false);

    }


    public void CarDestroyed()
    {
        if (tooltipPanelInstance != null)
        {
            Destroy(tooltipPanelInstance);
            tooltipPanelInstance = null;
        }

    }
}
