using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed when walking")]
    [SerializeField] private float walkSpeed = 6f;
    [Tooltip("Increased movement speed when sprinting")]
    [SerializeField] private float runSpeed = 10f;
    [Tooltip("Gravity force applied to the character")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [Tooltip("Mouse sensitivity for camera rotation")]
    [SerializeField] private float mouseSensitivity = 200f;
    [Tooltip("Reference to the player's camera transform")]
    [SerializeField] private Transform playerCamera;
    [Tooltip("Maximum vertical angle the player can look up/down")]
    [SerializeField] private float maxLookAngle = 90f;

    [Header("Zoom Settings")]
    [Tooltip("Field of view when zooming")]
    [SerializeField] private float zoomFOV = 30f;
    [Tooltip("Movement speed multiplier while zoomed")]
    [SerializeField] private float zoomSpeedMultiplier = 0.5f;
    [Tooltip("Mouse sensitivity while zoomed")]
    [SerializeField] private float zoomMouseSensitivity = 100f;
    [Tooltip("Speed of zoom transition")]
    [SerializeField] private float zoomTransitionSpeed = 10f;

    [Header("Vehicle Settings")]
    [Tooltip("Engine sound effect component")]
    [SerializeField] private AudioSource engineSound;
    [Tooltip("Array of headlight renderers")]
    [SerializeField] private Renderer[] headlightsRenderers;
    [Tooltip("Array of rear light renderers")]
    [SerializeField] private Renderer[] rearLightsRenderers;

    [Header("Spotlight")]
    [Tooltip("Reference to the vehicle's spotlight")]
    [SerializeField] private Light Barlight1;
    [Tooltip("Minimum time between light flickers")]
    [SerializeField] private float flickerMinInterval = 0.05f;
    [Tooltip("Maximum time between light flickers")]
    [SerializeField] private float flickerMaxInterval = 0.2f;

    [Header("Lens Materials")]
    [Tooltip("Material for inactive lights")]
    [SerializeField] private Material blackLensMaterial;
    [Tooltip("Material for active front lights")]
    [SerializeField] private Material frontLightMaterial;
    [Tooltip("Material for active rear lights")]
    [SerializeField] private Material rearLightMaterial;

    // Component references
    private CharacterController characterController;
    private Camera playerCameraComponent;

    // Movement variables
    private Vector3 velocity;                 // controls movement velocity
    private float xRotation = 0f;             // controls camera rotation

    // Zoom state
    private bool isZooming = false;           // Flags for zoom mode to operate correctly
    private float normalFOV;                  // Default field of view
    private float normalMouseSensitivity;     // Default mouse sensitivity

    // Vehicle state
    private Coroutine flickerCoroutine;       // Reference to active flicker routine controls the amount of flickering everytime the car is started
    private bool canStartCar = false;         // Allows Player to interact with vehicle
    private bool isEngineOn = false;          // Vehicle engine state on or off

    void Start()
    {
        // Initializes character controller component that allows for easy movement
        characterController = GetComponent<CharacterController>();
        
        // Sets the cursor state
        Cursor.lockState = CursorLockMode.Locked;
        //cursor does not show up in first person
        Cursor.visible = false;

        // Caches the camera component and initial settings
        playerCameraComponent = playerCamera.GetComponent<Camera>();
        normalFOV = playerCameraComponent.fieldOfView;
        normalMouseSensitivity = mouseSensitivity;

        // Initialize all lights to off state, which makes sure the different lights of the car start out off when the game starts
        foreach (var rend in headlightsRenderers)
            rend.material = blackLensMaterial;
        foreach (var rend in rearLightsRenderers)
            rend.material = blackLensMaterial;

        // Ensures that the spotlight starts disabled, gives the effect of a show case so that when the car starts the spotlight turns on
        if (Barlight1 != null) Barlight1.enabled = false;
    }

    void Update()
    {
        // Handles all input and physics updates, mouse movements and other physical inputs
        HandleMouseLook();
        HandleMovement();
        HandleVehicleControls();
        HandleZoom();
    }

    // it controls mouse input for the cameras rotation
    private void HandleMouseLook()
    {
        // Gets the raw mouse input and scale by sensitivity for smoothness
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotates player horizontally
        transform.Rotate(Vector3.up * mouseX);
        
        // Calculates and clamps the vertical rotation for smooth firstperson action when moving the cam
        xRotation = Mathf.Clamp(xRotation - mouseY, -maxLookAngle, maxLookAngle);
        
        // Apply vertical rotation to camera
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    
    // controls the player movement and gravity
    private void HandleMovement()
    {
        // Resets the vertical velocity when the character is grounded
        if (characterController.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Gets the movement input axes
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // This is what Calculates the movement direction relative to the players orientation
        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);  // Normalize diagonal movement

        // This Adjusts the speed based on zoom state, so that its not too slow or too fast while zoomed in
        float currentWalkSpeed = isZooming ? walkSpeed * zoomSpeedMultiplier : walkSpeed;
        float currentRunSpeed = isZooming ? runSpeed * zoomSpeedMultiplier : runSpeed;

        // contorls current speed based on sprint input
        float speed = Input.GetKey(KeyCode.LeftShift) ? currentRunSpeed : currentWalkSpeed;
        
        //  movement
        characterController.Move(move * speed * Time.deltaTime);

        // gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // Handles vehicle control inputs (engine start/stop)

    private void HandleVehicleControls()
    {
        // Starts the engine conditions: F pressed, in range, no active flicker, engine off
        if (Input.GetKeyDown(KeyCode.F) && canStartCar && flickerCoroutine == null && !isEngineOn)
        {
            isEngineOn = true;
            flickerCoroutine = StartCoroutine(LightsSequence());
        }
        // Stop engine conditions: G pressed
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // Stop any active flicker routine
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }

            isEngineOn = false;
            ToggleAllLights(false);

            // Stop engine sound if playing
            if (engineSound != null)
                engineSound.Stop();
        }
    }

    // Handles the zoom functionality (FOV and sensitivity changes)
    private void HandleZoom()
    {
        // Toggle zoom state on Z press
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isZooming = !isZooming;
        }

        // Smoothly transition FOV how it looks when zooming in and out and how smooth it is
        playerCameraComponent.fieldOfView = Mathf.Lerp(
            playerCameraComponent.fieldOfView,
            isZooming ? zoomFOV : normalFOV,
            zoomTransitionSpeed * Time.deltaTime
        );

        // Smoothly adjust mouse sensitivity
        mouseSensitivity = Mathf.Lerp(
            mouseSensitivity,
            isZooming ? zoomMouseSensitivity : normalMouseSensitivity,
            zoomTransitionSpeed * Time.deltaTime
        );
    }

    
    // Coroutine that handles the light flicker sequence when starting engine
       private IEnumerator LightsSequence()
    {
        // Three flicker cycles
        for (int i = 0; i < 3; i++)
        {
            ToggleAllLights(true);
            yield return new WaitForSeconds(Random.Range(flickerMinInterval, flickerMaxInterval));
            ToggleAllLights(false);
            yield return new WaitForSeconds(Random.Range(flickerMinInterval, flickerMaxInterval));
        }

        // Final light activation
        ToggleAllLights(true);
        if (engineSound != null)
            engineSound.Play();

        flickerCoroutine = null;
    }


    // Toggles all vehicle lights and materials
    // <param name="on">Whether to activate lights</param>
    private void ToggleAllLights(bool on)
    {
        // Selects the appropriate materials based on state
        Material frontMat = on ? frontLightMaterial : blackLensMaterial;
        Material rearMat = on ? rearLightMaterial : blackLensMaterial;

        // Apply to all headlights
        foreach (var rend in headlightsRenderers)
            rend.material = frontMat;

        // Apply to all rear lights
        foreach (var rend in rearLightsRenderers)
            rend.material = rearMat;

        // Toggle spotlight
        if (Barlight1 != null)
            Barlight1.enabled = on;
    }

    
    // Enables/disables vehicle interaction capability
    
    // <param name="canStart">Whether vehicle can be started</param>
    public void EnableCarInteraction(bool canStart)
    {
        canStartCar = canStart;
    }

    
    // Handles the GUI display of control prompts

    void OnGUI()
    {
        // Configure GUI style
        GUIStyle messageStyle = new GUIStyle(GUI.skin.label);
        messageStyle.fontSize = 24;
        messageStyle.normal.textColor = Color.white;

        // Display vehicle interaction prompt when in range
        if (canStartCar)
        {
            string carMessage = !isEngineOn ? "Press F to start the engine." : "Press G to cut off the engine.";
            GUI.Label(new Rect(10, 10, 400, 50), carMessage, messageStyle);
        }

        // Permanent movement controls display
        string movementMessage = "Movement: WASD or Arrow Keys";
        GUI.Label(new Rect(10, Screen.height - 40, 400, 50), movementMessage, messageStyle);

        // Permanent zoom instructions display
        string zoomMessage = "Press Z to zoom in and examine the cars\n(You can pass through them to view the interiors)";
        GUI.Label(
            new Rect(Screen.width - 410, Screen.height - 60, 400, 60),
            zoomMessage,
            messageStyle
        );
    }
}
