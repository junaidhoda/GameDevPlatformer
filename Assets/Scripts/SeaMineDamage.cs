
using UnityEngine;

public class SeaMineDamage : MonoBehaviour
{
    public int damage = 25;

    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject hitObject)
    {
        if (hasHit) return;

        PlayerHealth player = hitObject.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            hasHit = true;
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (hitObject.name == "Ground")
        {
            Destroy(gameObject, 0.5f);
        }
    }
}
