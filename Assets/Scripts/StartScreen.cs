using UnityEngine;

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

    void Start()
    {
        // Make sure time is normal when the scene begins.
        Time.timeScale = 1f;

        // Show the start screen first.
        if (startScreen != null)
        {
            startScreen.SetActive(true);
        }

        // Hide all car health UI canvases until the player starts the game.
        foreach (GameObject healthUI in carHealthUIs)
        {
            if (healthUI != null)
            {
                healthUI.SetActive(false);
            }
        }
    }

    public void StartGame()
    {
        // Hide the start screen.
        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        // Show all car health UI canvases now that gameplay has begun.
        foreach (GameObject healthUI in carHealthUIs)
        {
            if (healthUI != null)
            {
                healthUI.SetActive(true);
            }
        }

        // Teleport the XR Origin to the garage rage room spawn point.
        if (xrOrigin != null && garageSpawnPoint != null)
        {
            xrOrigin.position = garageSpawnPoint.position;
            xrOrigin.rotation = garageSpawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("XR Origin or Garage Spawn Point is missing in StartScreenManager.");
        }
    }
}
