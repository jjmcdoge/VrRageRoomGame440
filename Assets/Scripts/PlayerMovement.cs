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

    [Header("Look Settings (Arrow Keys)")]
    [Tooltip("Degrees per second for arrow-key camera turning (Yaw/Pitch)")]
    [SerializeField] private float lookSpeed = 140f;
    [Tooltip("Reference to the player's camera transform")]
    [SerializeField] private Transform playerCamera;
    [Tooltip("Maximum vertical angle the player can look up/down")]
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Zoom Settings")]
    [Tooltip("Field of view when zooming")]
    [SerializeField] private float zoomFOV = 30f;
    [Tooltip("Movement speed multiplier while zoomed")]
    [SerializeField] private float zoomSpeedMultiplier = 0.5f;
    [Tooltip("Look speed multiplier while zoomed")]
    [SerializeField] private float zoomLookMultiplier = 0.6f;
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
    private Vector3 velocity;
    private float xRotation = 0f;

    // Zoom state
    private bool isZooming = false;
    private float normalFOV;

    // Vehicle state
    private Coroutine flickerCoroutine;
    private bool canStartCar = false;
    private bool isEngineOn = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Arrow-key camera means we don't need locked cursor, but leaving it hidden is fine
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCameraComponent = playerCamera.GetComponent<Camera>();
        normalFOV = playerCameraComponent.fieldOfView;

        // Initialize all lights to off state
        foreach (var rend in headlightsRenderers)
            if (rend != null) rend.material = blackLensMaterial;

        foreach (var rend in rearLightsRenderers)
            if (rend != null) rend.material = blackLensMaterial;

        // Spotlight starts disabled
        if (Barlight1 != null) Barlight1.enabled = false;
    }

    void Update()
    {
        HandleArrowKeyLook();  // Camera with arrow keys
        HandleMovement();      // WASD movement
        HandleVehicleControls();
        HandleZoom();
    }

    // Camera look controlled by arrow keys (Left/Right = yaw, Up/Down = pitch)
    private void HandleArrowKeyLook()
    {
        float yawInput = 0f;
        float pitchInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))  yawInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) yawInput =  1f;

        if (Input.GetKey(KeyCode.UpArrow))    pitchInput =  1f;
        if (Input.GetKey(KeyCode.DownArrow))  pitchInput = -1f;

        float currentLookSpeed = isZooming ? lookSpeed * zoomLookMultiplier : lookSpeed;

        float yaw = yawInput * currentLookSpeed * Time.deltaTime;
        float pitch = pitchInput * currentLookSpeed * Time.deltaTime;

        // Yaw on player body
        transform.Rotate(Vector3.up * yaw);

        // Pitch on camera
        xRotation = Mathf.Clamp(xRotation + pitch, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // WASD movement + gravity
    private void HandleMovement()
    {
        if (characterController.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // WASD uses the default Horizontal/Vertical axes
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);

        float currentWalkSpeed = isZooming ? walkSpeed * zoomSpeedMultiplier : walkSpeed;
        float currentRunSpeed = isZooming ? runSpeed * zoomSpeedMultiplier : runSpeed;

        float speed = Input.GetKey(KeyCode.LeftShift) ? currentRunSpeed : currentWalkSpeed;

        characterController.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // Vehicle control inputs (engine start/stop)
    private void HandleVehicleControls()
    {
        // Start engine: F pressed, in range, no active flicker, engine off
        if (Input.GetKeyDown(KeyCode.F) && canStartCar && flickerCoroutine == null && !isEngineOn)
        {
            isEngineOn = true;
            flickerCoroutine = StartCoroutine(LightsSequence());
        }
        // Stop engine: G pressed
        else if (Input.GetKeyDown(KeyCode.G))
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }

            isEngineOn = false;
            ToggleAllLights(false);

            if (engineSound != null)
                engineSound.Stop();
        }
    }

    // Zoom (FOV transition only; look is slowed via zoomLookMultiplier)
    private void HandleZoom()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            isZooming = !isZooming;

        playerCameraComponent.fieldOfView = Mathf.Lerp(
            playerCameraComponent.fieldOfView,
            isZooming ? zoomFOV : normalFOV,
            zoomTransitionSpeed * Time.deltaTime
        );
    }

    // Light flicker sequence when starting engine
    private IEnumerator LightsSequence()
    {
        for (int i = 0; i < 3; i++)
        {
            ToggleAllLights(true);
            yield return new WaitForSeconds(Random.Range(flickerMinInterval, flickerMaxInterval));
            ToggleAllLights(false);
            yield return new WaitForSeconds(Random.Range(flickerMinInterval, flickerMaxInterval));
        }

        ToggleAllLights(true);

        if (engineSound != null)
            engineSound.Play();

        flickerCoroutine = null;
    }

    // Toggles all vehicle lights and materials
    private void ToggleAllLights(bool on)
    {
        Material frontMat = on ? frontLightMaterial : blackLensMaterial;
        Material rearMat = on ? rearLightMaterial : blackLensMaterial;

        foreach (var rend in headlightsRenderers)
            if (rend != null) rend.material = frontMat;

        foreach (var rend in rearLightsRenderers)
            if (rend != null) rend.material = rearMat;

        if (Barlight1 != null)
            Barlight1.enabled = on;
    }

    // Enables/disables vehicle interaction capability
    public void EnableCarInteraction(bool canStart)
    {
        canStartCar = canStart;
    }

    void OnGUI()
    {
        // Keep UI in-frame with padding
        float pad = 10f;

        GUIStyle messageStyle = new GUIStyle(GUI.skin.label);
        messageStyle.fontSize = 24;
        messageStyle.normal.textColor = Color.white;
        messageStyle.wordWrap = true;

        // Vehicle prompt (top-left)
        if (canStartCar)
        {
            string carMessage = !isEngineOn ? "Press F to start the engine." : "Press G to cut off the engine.";
            GUI.Label(new Rect(pad, pad, Screen.width - pad * 2f, 60f), carMessage, messageStyle);
        }

        // Controls (bottom-left)
        GUI.Label(
            new Rect(pad, Screen.height - 40f - pad, Screen.width * 0.6f, 40f),
            "Move: WASD   Sprint: Left Shift   Look: Arrow Keys",
            messageStyle
        );

        // Zoom instructions (bottom-right) — width clamped so it stays on screen
        float rightWidth = Mathf.Min(520f, Screen.width - pad * 2f);
        float rightX = Screen.width - rightWidth - pad;

        GUI.Label(
            new Rect(rightX, Screen.height - 70f - pad, rightWidth, 70f),
            "Press Z to zoom in and examine the cars\n(You can pass through them to view the interiors)",
            messageStyle
        );
    }
}