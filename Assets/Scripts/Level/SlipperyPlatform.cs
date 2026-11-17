using UnityEngine;

// Slick, kelp-covered platform. While the player stands on it they slide:
// speeding up and slowing down takes time, so it's easy to overshoot the edge.
// It also wobbles gently. PlayerPlatformContact tells PlayerMovement when
// the player is standing on one.
public class SlipperyPlatform : MonoBehaviour
{
    [Tooltip("How quickly the player can change speed on this platform (lower = more slippery)")]
    public float grip = 5f;

    [Header("Wobble")]
    public float wobbleAngle = 3f;
    public float wobbleSpeed = 1.2f;

    private Quaternion startRotation;
    private float offset;

    void Start()
    {
        startRotation = transform.localRotation;
        offset = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (wobbleAngle <= 0f) return;

        float t = Time.time * wobbleSpeed + offset;
        transform.localRotation = startRotation * Quaternion.Euler(
            Mathf.Sin(t) * wobbleAngle,
            0f,
            Mathf.Sin(t * 0.7f + 1.3f) * wobbleAngle);
    }
}
