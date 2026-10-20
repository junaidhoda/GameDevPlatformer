using UnityEngine;

// Big invisible area under the level. Falling into it sends the player
// back to the last respawn point they reached.
public class KillZone : MonoBehaviour
{
    public int fallDamage = 0;

    private static Vector3 respawnPosition;

    void Start()
    {
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null) respawnPosition = player.transform.position;
    }

    public static void SetRespawn(Vector3 position)
    {
        respawnPosition = position;
    }

    void OnTriggerEnter(Collider other)
    {
        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller == null) return;

        Respawn(controller);

        if (fallDamage > 0)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(fallDamage);
        }
    }

    public static void Respawn(CharacterController controller)
    {
        // CharacterController must be off to move it directly
        controller.enabled = false;
        controller.transform.position = respawnPosition;
        controller.enabled = true;

        PlayerMovement movement = controller.GetComponent<PlayerMovement>();
        if (movement != null) movement.Launch(0f);
    }
}
