using UnityEngine;

// Moves back and forth between its start position and start + moveOffset.
// PlayerPlatformContact uses Delta to carry the player along.
public class MovingPlatform : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(8f, 0f, 0f);
    public float speed = 2.5f;

    public Vector3 Delta { get; private set; }

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float distance = moveOffset.magnitude;
        if (distance < 0.01f)
        {
            Delta = Vector3.zero;
            return;
        }

        float t = Mathf.PingPong(Time.time * speed / distance, 1f);
        t = Mathf.SmoothStep(0f, 1f, t);

        Vector3 newPosition = startPosition + moveOffset * t;
        Delta = newPosition - transform.position;
        transform.position = newPosition;
    }
}
