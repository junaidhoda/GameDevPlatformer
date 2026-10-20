using UnityEngine;

// Invisible area on a safe platform. When the player walks in, this spot
// becomes the place they come back to after falling.
// Place the object about 1.2 units above the platform surface.
public class RespawnPoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;
        KillZone.SetRespawn(transform.position);
    }
}
