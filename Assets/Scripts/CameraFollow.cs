
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;

    private Vector3 offset;
    private Vector3 followPosition;

    // Camera shake (used by jump pads)
    private static float shakeStrength;
    private static float shakeEndTime;
    private static float shakeLength = 0.01f;

    void Start()
    {
        // Remember the camera's starting position relative to the player
        offset = transform.position - player.position;
        followPosition = transform.position;
    }

    // Shake the camera for a moment. Strength is how far it moves, in units.
    public static void Shake(float strength, float duration)
    {
        shakeStrength = strength;
        shakeLength = Mathf.Max(0.01f, duration);
        shakeEndTime = Time.time + duration;
    }

    void LateUpdate()
    {
        // Follow the player while keeping the same camera angle
        Vector3 targetPosition = player.position + offset;

        followPosition = Vector3.Lerp(
            followPosition,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        // Shake fades out over its duration
        Vector3 shake = Vector3.zero;
        float timeLeft = shakeEndTime - Time.time;
        if (timeLeft > 0f)
            shake = Random.insideUnitSphere * shakeStrength * (timeLeft / shakeLength);

        transform.position = followPosition + shake;
    }
}
