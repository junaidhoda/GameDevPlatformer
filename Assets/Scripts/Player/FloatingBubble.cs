using UnityEngine;

// A small bubble that drifts upwards with a little wobble, shrinks and disappears.
// Used for the diver's breathing bubbles and the jump pad trail.
public class FloatingBubble : MonoBehaviour
{
    private float riseSpeed;
    private float lifetime;
    private float startTime;
    private float wobbleOffset;
    private Vector3 startScale;

    public void Init(float rise, float life)
    {
        riseSpeed = rise;
        lifetime = life;
        startTime = Time.time;
        wobbleOffset = Random.Range(0f, 10f);
        startScale = transform.localScale;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float age = Time.time - startTime;
        float wobble = Mathf.Sin((age + wobbleOffset) * 8f) * 0.3f;
        transform.position += new Vector3(wobble, riseSpeed, 0f) * Time.deltaTime;
        transform.localScale = Vector3.Lerp(startScale, startScale * 0.3f, age / Mathf.Max(0.01f, lifetime));
    }
}
