using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private PlaneRemover planeRemover;

    public void StartApp()
    {
        spawnManager.StartPlaneDetection();
        
    }

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToHome()
    {
        spawnManager.StopPlaneDetection();
        planeRemover.ResetARSession();
        spawnManager.OnDeleteCarClicked();
    }

}
