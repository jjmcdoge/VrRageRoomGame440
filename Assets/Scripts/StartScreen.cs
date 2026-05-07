using UnityEngine;
using UnityEngine.InputSystem;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The start screen canvas or panel shown at the beginning.")]
    public GameObject startScreen;

    [Tooltip("Drag all car health canvas objects here so they stay hidden until Start is pressed.")]
    public GameObject[] carHealthUIs;

    [Header("Teleport References")]
    [Tooltip("Drag the XR Origin / XR Rig root here.")]
    public Transform xrOrigin;

    [Tooltip("Drag the garage spawn point here.")]
    public Transform garageSpawnPoint;

    [Header("VR Input")]
    [Tooltip("Assign the A button input action here.")]
    public InputActionProperty startButton;

    // Prevents StartGame from running multiple times.
    private bool gameStarted = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (startScreen != null)
        {
            startScreen.SetActive(true);
        }

        foreach (GameObject healthUI in carHealthUIs)
        {
            if (healthUI != null)
            {
                healthUI.SetActive(false);
            }
        }

        // Enable the VR input action.
        startButton.action.Enable();
    }

    void Update()
    {
        // When A button is pressed, start the game.
        if (!gameStarted && startButton.action.WasPressedThisFrame())
        {
            gameStarted = true;
            StartGame();
        }
    }

    public void StartGame()
    {
        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        foreach (GameObject healthUI in carHealthUIs)
        {
            if (healthUI != null)
            {
                healthUI.SetActive(true);
            }
        }

        if (xrOrigin != null && garageSpawnPoint != null)
        {
            Vector3 cameraOffset = Camera.main.transform.position - xrOrigin.position;

            Vector3 targetPosition =
                garageSpawnPoint.position -
                new Vector3(cameraOffset.x, 0f, cameraOffset.z);

            xrOrigin.position = targetPosition;
            xrOrigin.rotation = garageSpawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("XR Origin or Garage Spawn Point is missing in StartScreenManager.");
        }
    }
}