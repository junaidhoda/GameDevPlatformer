using UnityEngine;

// Flies in a straight line, damages the player on hit,
// and disappears when it hits anything solid or runs out of time.
public class ArrowProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private int damage;
    private GameObject owner;
    private bool hasHit;

    public void Init(Vector3 startVelocity, int hitDamage, float lifetime, GameObject shooter)
    {
        velocity = startVelocity;
        damage = hitDamage;
        owner = shooter;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;
        if (other.isTrigger) return; // ignore treasure, zones, etc.

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null) player.TakeDamage(damage);

        hasHit = true;
        Destroy(gameObject);
    }
}
