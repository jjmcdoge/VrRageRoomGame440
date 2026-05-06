using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [SerializeField] int damageAmount = 1;
    [SerializeField] float hitCooldown = 1f;

    private float nextHitTime;

    private void OnCollisionEnter(Collision collision)
    {
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject hitObject)
    {
        if (Time.time < nextHitTime)
        {
            return;
        }

        CarHealth carHealth = hitObject.GetComponentInParent<CarHealth>();

        if (carHealth != null)
        {
            Debug.Log("Weapon hit car");

            carHealth.TakeDamage(damageAmount);

            // Stops the same touch from counting multiple times too fast.
            nextHitTime = Time.time + hitCooldown;
        }
    }
}