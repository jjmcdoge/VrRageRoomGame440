using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;

public class ArmSwingMovement : MonoBehaviour
{
    [Header("References")]
    public XROrigin xrOrigin;
    public Transform head;
    public InputActionProperty leftVelocityAction;
    public InputActionProperty rightVelocityAction;

    [Header("Movement Settings")]
    public float swingThreshold = 1.2f;   // Minimum arm speed to count as a swing
    public float moveSpeed = 3f;          // Movement speed multiplier
    public float cooldownTime = 0.2f;     // Prevents constant triggering

    private float cooldownTimer = 0f;

    void Update()
    {
        cooldownTimer -= Time.deltaTime; 

        Vector3 leftVel = leftVelocityAction.action.ReadValue<Vector3>(); 
        Vector3 rightVel = rightVelocityAction.action.ReadValue<Vector3>();

        float leftSpeed = leftVel.magnitude;
        float rightSpeed = rightVel.magnitude;

        bool leftSwing = leftSpeed > swingThreshold;
        bool rightSwing = rightSpeed > swingThreshold;

        if ((leftSwing || rightSwing) && cooldownTimer <= 0f)
        {
            MoveForward();
            cooldownTimer = cooldownTime;
        }
    }

    void MoveForward()
    {
        Vector3 forward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        xrOrigin.transform.position += forward * moveSpeed * Time.deltaTime;
    }
}
