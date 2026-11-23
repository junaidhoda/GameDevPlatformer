using UnityEngine;

// Launches the player upwards when they land on it.
public class JumpPad : MonoBehaviour
{
    public float launchSpeed = 13f;
    public float cooldown = 0.3f;

    private float nextLaunch;
    private Vector3 normalScale;

    void Start()
    {
        normalScale = transform.localScale;
    }

    public void Launch(GameObject player)
    {
        if (Time.time < nextLaunch) return;
        nextLaunch = Time.time + cooldown;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.Launch(launchSpeed);

        // Small squash so it feels springy
        transform.localScale = new Vector3(normalScale.x, normalScale.y * 0.5f, normalScale.z);

        // Rocket feel: a burst of bubbles under the player and a little camera shake
        Vector3 feet = player.transform.position + Vector3.down * 0.9f;
        DashKillable.Pop(feet, new Color(0.75f, 0.95f, 1f));
        DashKillable.Pop(feet, new Color(0.3f, 0.9f, 0.35f));
        CameraFollow.Shake(0.25f, 0.3f);

        // Leave a short streak behind the player on the way up
        RocketTrail.Begin(player, launchSpeed / 14f);
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, normalScale, 10f * Time.deltaTime);
    }
}
