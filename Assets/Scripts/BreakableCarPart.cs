using UnityEngine;

public class BreakableCarPart : MonoBehaviour
{
    [Header("Break Settings")]
    public float breakForce = 5f;
    public float upwardForce = 2f;
    public float disappearAfterSeconds = 6f;

    private Rigidbody rb;
    private bool hasBroken = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBroken)
        {
            return;
        }

        Vector3 hitDirection = collision.relativeVelocity.normalized;
        HitPart(hitDirection);
    }

    public void HitPart(Vector3 hitDirection)
    {
        hasBroken = true;

        transform.parent = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(hitDirection * breakForce + Vector3.up * upwardForce, ForceMode.Impulse);
        }

        Destroy(gameObject, disappearAfterSeconds);
    }
}