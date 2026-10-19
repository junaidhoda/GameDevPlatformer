
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;

    private Vector3 offset;

    void Start()
    {
        // Remember the camera's starting position relative to the player
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        // Follow the player while keeping the same camera angle
        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
