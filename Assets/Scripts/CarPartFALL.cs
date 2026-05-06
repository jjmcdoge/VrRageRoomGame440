using UnityEngine;

public class CarPartFALL : MonoBehaviour
{
    public Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void Deteach()
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.WakeUp();

        Destroy(gameObject, 3f);
    }
    void Update()
    {
        
    }
}
