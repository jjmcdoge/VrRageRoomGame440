using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class VRLocomotion : MonoBehaviour
{
    public float moveSpeed = 2f;
    public InputActionProperty moveInput; // Vector2 from left joystick
    private XROrigin xrOrigin;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
    }

    void Update()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();

        // Convert joystick direction into world direction based on head orientation
        Transform head = xrOrigin.Camera.transform;

        Vector3 forward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 right = new Vector3(head.right.x, 0, head.right.z).normalized;

        Vector3 move = forward * input.y + right * input.x;

        xrOrigin.transform.position += move * moveSpeed * Time.deltaTime;
    }
}

