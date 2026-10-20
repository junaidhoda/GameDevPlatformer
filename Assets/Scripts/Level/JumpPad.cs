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
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, normalScale, 10f * Time.deltaTime);
    }
}
