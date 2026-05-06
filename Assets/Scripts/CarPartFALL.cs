using UnityEngine;

public class CarPartFALL : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasDetached = false;

    [SerializeField] float destroyDelay = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void Deteach()
    {
        if (hasDetached)
        {
            return;
        }

        hasDetached = true;

        // Remove it from the car parent so it can fall by itself.
        transform.parent = null;

        if (rb != null)
        {
            // Turn physics on so it falls.
            rb.isKinematic = false;
            rb.useGravity = true;

            // Wake it up in case Unity had it sleeping.
            rb.WakeUp();
        }

        // Wait a few seconds after it falls, then destroy it.
        Destroy(gameObject, destroyDelay);
    }
}