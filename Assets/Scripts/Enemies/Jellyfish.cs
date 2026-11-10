using UnityEngine;

// Floats slowly up and down. Placed in gaps so the player has to time
// their jump. Damage comes from ContactDamage on the same object.
public class Jellyfish : MonoBehaviour
{
    public float bobHeight = 1.6f;
    public float bobSpeed = 0.8f;
    [Range(0f, 1f)] public float phase = 0f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float wave = Mathf.Sin((Time.time * bobSpeed + phase) * Mathf.PI * 2f);
        transform.position = startPosition + Vector3.up * wave * bobHeight;
    }
}
