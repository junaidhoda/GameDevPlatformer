using UnityEngine;

// Hurts the player while they are touching this enemy.
// Needs a trigger collider and a kinematic Rigidbody on the same object.
public class ContactDamage : MonoBehaviour
{
    public int damage = 15;
    public float hitCooldown = 1f;

    private float nextHit;

    void OnTriggerStay(Collider other)
    {
        if (Time.time < nextHit) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        player.TakeDamage(damage);
        nextHit = Time.time + hitCooldown;
    }
}
