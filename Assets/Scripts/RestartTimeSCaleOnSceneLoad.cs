using UnityEngine;

public class RestartTimeScaleOnSceneLoad : MonoBehaviour
{
    void Awake()
    {
        // Always reset time scale when the scene starts.
        // This prevents the game from staying frozen if you hit Play again
        // after a previous win set Time.timeScale to 0.
        Time.timeScale = 1f;
    }
}