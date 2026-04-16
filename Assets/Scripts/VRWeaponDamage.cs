using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRWeaponDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("How much damage this weapon does every successful hit.")]
    public int damageAmount = 1;

    [Tooltip("How long to wait before the weapon can deal damage again.")]
    public float damageCooldown = 0.5f;

    // Reference to the XR Grab Interactable component on this weapon.
    // This is what lets us know whether the player is currently holding it.
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    // This becomes true when the player grabs the weapon,
    // and false when they let go.
    private bool isHeld = false;

    // Stores the last time damage was dealt.
    // This is used to prevent very rapid repeated damage.
    private float lastDamageTime = -999f;

    void Awake()
    {
        // Grab the XRGrabInteractable from this object.
        // Make sure the weapon has one in the Inspector.
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void OnEnable()
    {
        // Only add listeners if the component exists.
        if (grabInteractable != null)
        {
            // Called when the player grabs the weapon.
            grabInteractable.selectEntered.AddListener(OnGrab);

            // Called when the player releases the weapon.
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        // Remove listeners when disabled so we do not create duplicate event calls.
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // The player is now holding the weapon.
        isHeld = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // The player is no longer holding the weapon.
        isHeld = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        // If the weapon is not being held, do not apply damage.
        if (!isHeld)
        {
            return;
        }

        // Make sure enough time has passed since the last hit.
        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        // Try to find a CarHealth script on the object we hit.
        CarHealth carHealth = collision.gameObject.GetComponent<CarHealth>();

        // If the object we hit has CarHealth, apply damage.
        if (carHealth != null)
        {
            carHealth.TakeDamage(damageAmount);

            // Store the current time so the cooldown begins now.
            lastDamageTime = Time.time;

            Debug.Log("Weapon hit the car and dealt " + damageAmount + " damage.");
        }
    }
}