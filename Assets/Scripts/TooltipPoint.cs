using UnityEngine;

public class TooltipPoint : MonoBehaviour
{
    [Header("Tooltip Info")]
    public string title = "Titolo Tooltip";
    [TextArea] public string infoText = "Informazione sul componente";

    [Header("Animazione")]
    [SerializeField] private float floatAmplitude = 0.05f; // Oscillazione verticale
    [SerializeField] private float floatSpeed = 1f;        // Velocità oscillazione
    [SerializeField] private float rotationSpeed = 30f;    // Velocità rotazione Y

    private Vector3 startPosition;
    private bool isPaused = false;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        if (isPaused) return;

        // Fluttuazione verticale
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

        // Rotazione continua
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }


    // Chiamato dal TooltipManager quando il pannello si apre
    public void PauseAnimation()
    {
        isPaused = true;
    }

    // Chiamato dal TooltipManager quando il pannello si chiude
    public void ResumeAnimation()
    {
        isPaused = false;
    }
}
