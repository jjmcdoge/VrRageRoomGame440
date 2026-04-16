using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The start screen canvas or panel shown at the beginning.")]
    public GameObject startScreen;

    [Tooltip("Drag the car health canvas or health text object here so it stays hidden until Start is pressed.")]
    public GameObject carHealthUI;

    void Start()
    {
        // Make sure time is normal when the scene begins.
        Time.timeScale = 1f;

        // Show the start screen first.
        if (startScreen != null)
        {
            startScreen.SetActive(true);
        }

        // Hide the car health UI until the player starts the game.
        if (carHealthUI != null)
        {
            carHealthUI.SetActive(false);
        }
    }

    public void StartGame()
    {
        // Hide the start screen.
        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        // Show the car health UI now that gameplay has begun.
        if (carHealthUI != null)
        {
            carHealthUI.SetActive(true);
        }
    }
}