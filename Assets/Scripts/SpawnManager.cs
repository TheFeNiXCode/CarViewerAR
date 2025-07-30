using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject ghostPrefab;

    [Header("UI Elements")]
    [SerializeField] private Button selectCarButton;
    [SerializeField] private Button confirmPlacementButton;
    [SerializeField] private Button deleteCarButton;
    [SerializeField] private Button exitManipulationButton; 
    [SerializeField] private GameObject manipulationPanel;  
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Slider rotateSlider;
    [SerializeField] private Toggle manipulationModeToggle;
    [SerializeField] private GameObject panelMove;

    [Header("Scale Settings")]
    [SerializeField] public float ghostScale = 0.3f;
    [SerializeField] public float carScale = 0.3f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject ghostInstance;
    private GameObject spawnedCar;
    private bool isPlacing = false;
    private Pose currentPose;
    private bool poseIsValid = false;
    private bool isManipulating = false;
    private bool planeLocked = false;

    void Start()
    {
        // Bottone per selezionare e iniziare a posizionare
        selectCarButton.onClick.AddListener(OnSelectCarClicked);
        confirmPlacementButton.onClick.AddListener(OnConfirmPlacementClicked);
        deleteCarButton.onClick.AddListener(OnDeleteCarClicked);
        exitManipulationButton.onClick.AddListener(OnExitManipulationClicked);

        // Slider listeners
        heightSlider.onValueChanged.AddListener(OnHeightChanged);
        scaleSlider.onValueChanged.AddListener(OnScaleChanged);
        rotateSlider.onValueChanged.AddListener(OnRotateChanged);

        // UI iniziale
        manipulationPanel.SetActive(false);
        panelMove.SetActive(false);
        deleteCarButton.gameObject.SetActive(false);

        StopPlaneDetection();
    }

    void Update()
    {
        if (isPlacing)
        {
            UpdateGhost();

            if (IsPrimaryClickDown() && poseIsValid && !IsPointerOverUI())
            {
                OnConfirmPlacementClicked();
            }
        }

        // Se tocco lo schermo (e non sono in placing), controllo selezione oggetto
        if (IsPrimaryClickDown() && !isPlacing && !IsPointerOverUI())
        {
            CheckModelSelection(GetClickPosition());
        }

        // Se modalità manipolazione con toggle ON, sposto oggetto
        if (isManipulating && manipulationModeToggle.isOn && IsPrimaryClickHeld() && !IsPointerOverUI())
        {
            MoveObjectOnPlane(GetClickPosition());
        }
    }

    // Ignora click sulla UI
    private bool IsPointerOverUI()
    {
        if (Application.isMobilePlatform)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        else
            return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPrimaryClickDown()
    {
        return (Application.isMobilePlatform && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) ||
               (!Application.isMobilePlatform && Input.GetMouseButtonDown(0));
    }

    private bool IsPrimaryClickHeld()
    {
        return (Application.isMobilePlatform && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) ||
               (!Application.isMobilePlatform && Input.GetMouseButton(0));
    }

    private Vector2 GetClickPosition()
    {
        return Application.isMobilePlatform ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
    }

    private void OnSelectCarClicked()
    {
        isPlacing = true;
        planeLocked = false;

        manipulationPanel.SetActive(false);

        StartPlaneDetection();


        if (ghostInstance == null)
        {
            ghostInstance = Instantiate(ghostPrefab);
            ghostInstance.transform.localScale = Vector3.one * ghostScale;

            StartCoroutine(GhostPulseAnimation());
        }
        ghostInstance.SetActive(false);
    }

    private void OnConfirmPlacementClicked()
    {
        if (!poseIsValid) return;

        if (spawnedCar == null)
        {
            spawnedCar = Instantiate(carPrefab, currentPose.position, currentPose.rotation);
            spawnedCar.transform.localScale = Vector3.one * carScale;

            StartCoroutine(SpawnPopInAnimation(spawnedCar.transform));
        }
        /*else
        {
            spawnedCar.transform.SetPositionAndRotation(currentPose.position, currentPose.rotation);
        }*/

        ghostInstance.SetActive(false);
        isPlacing = false;
        
        planeLocked = true;
        StopPlaneDetection();

        panelMove.SetActive(true);
        deleteCarButton.gameObject.SetActive(true);
    }

    public void OnDeleteCarClicked()
    {
        if (spawnedCar != null)
        {
            Destroy(spawnedCar);
            spawnedCar = null;
            panelMove.SetActive(false);
            deleteCarButton.gameObject.SetActive(false);
            manipulationPanel.SetActive(false);
        }
    }
    private void OnExitManipulationClicked()
    {
        isManipulating = false;
        manipulationPanel.SetActive(false);
    }

    private void UpdateGhost()
    {
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            currentPose = hits[0].pose;
            poseIsValid = true;

            if (!ghostInstance.activeSelf)
                ghostInstance.SetActive(true);

            ghostInstance.transform.SetPositionAndRotation(currentPose.position, currentPose.rotation);
            SetGhostColor(Color.green);
        }
        else
        {
            poseIsValid = false;
            SetGhostColor(Color.red);
        }
    }

    private void SetGhostColor(Color color)
    {
        Renderer[] renderers = ghostInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                Color c = color;
                c.a = 0.5f;
                m.color = c;
            }
        }
    }

    private void CheckModelSelection(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Se l'oggetto ha tag "Info" oppure è su layer UI, ignora il click
            if (hit.collider.CompareTag("Info") || hit.collider.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return;
            }

            // Se clicco il modello (o un suo figlio), entro in manipolazione
            if (spawnedCar != null && (hit.collider.gameObject == spawnedCar || hit.collider.transform.IsChildOf(spawnedCar.transform)))
            {
                EnableManipulation();
            }
        }
    }

    private void EnableManipulation()
    {
        if (spawnedCar == null) return;

        isManipulating = true;

        if (manipulationModeToggle.isOn)
        {
            manipulationPanel.SetActive(false); // Modalità spostamento libero
        }
        else
        {
            manipulationPanel.SetActive(true); // Mostro slider

            // Aggiorno slider con i valori attuali

            heightSlider.value = spawnedCar.transform.position.y;
            scaleSlider.value = carScale;
            rotateSlider.value = spawnedCar.transform.eulerAngles.y;
        }
    }

    private void MoveObjectOnPlane(Vector2 position)
    {
        if (raycastManager.Raycast(position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            Vector3 newPos = pose.position;
            newPos.y = spawnedCar.transform.position.y; // Mantengo altezza invariata
            spawnedCar.transform.position = newPos;
        }
    }

    private void OnHeightChanged(float value)
    {
        if (spawnedCar != null)
        {
            Vector3 pos = spawnedCar.transform.position;
            pos.y = value;
            spawnedCar.transform.position = pos;
        }
    }

    private void OnScaleChanged(float value)
    {
        if (spawnedCar != null)
        {
            spawnedCar.transform.localScale = Vector3.one * value;
            
            carScale = value;
            ghostScale = value;

            ghostInstance.transform.localScale = Vector3.one * ghostScale;

        }
    }

    private void OnRotateChanged(float value)
    {
        if (spawnedCar != null)
        {
            spawnedCar.transform.rotation = Quaternion.Euler(0, value, 0);
        }
    }

    public void StartPlaneDetection()
    {
        planeManager.enabled = true;
        planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }

    }

    public void StopPlaneDetection()
    {
        planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
        planeManager.enabled = false;

        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }


    // Animazioni Ghost e Pop-in
    // Animazioni Ghost e Pop-in
    private IEnumerator GhostPulseAnimation()
    {
        while (true)
        {
            if (!ghostInstance.activeSelf) { yield return null; continue; }
            Vector3 baseScale = Vector3.one * ghostScale;

            yield return ScaleOverTime(ghostInstance.transform, baseScale, baseScale * 1.05f, 0.5f);
            yield return ScaleOverTime(ghostInstance.transform, baseScale * 1.05f, baseScale, 0.5f);
        }
    }

    private IEnumerator SpawnPopInAnimation(Transform obj)
    {
        obj.localScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * carScale;
        yield return ScaleOverTime(obj, Vector3.zero, targetScale, 0.3f);
    }

    private IEnumerator ScaleOverTime(Transform obj, Vector3 start, Vector3 end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            obj.localScale = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        obj.localScale = end;
    }
}
