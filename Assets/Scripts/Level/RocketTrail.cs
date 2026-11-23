using UnityEngine;

// Short streak of rising bubbles behind the player after a jump pad launch.
// Added by JumpPad and removes itself when finished.
public class RocketTrail : MonoBehaviour
{
    public float duration = 0.8f;
    public float spawnInterval = 0.04f;

    private float endTime;
    private float nextSpawn;

    public static void Begin(GameObject player, float strength)
    {
        RocketTrail trail = player.GetComponent<RocketTrail>();
        if (trail == null) trail = player.AddComponent<RocketTrail>();
        trail.endTime = Time.time + trail.duration * Mathf.Clamp(strength, 0.5f, 1.5f);
        trail.enabled = true;
    }

    void Update()
    {
        if (Time.time > endTime)
        {
            enabled = false;
            return;
        }

        if (Time.time < nextSpawn) return;
        nextSpawn = Time.time + spawnInterval;

        GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(bubble.GetComponent<Collider>());
        bubble.name = "RocketBubble";
        bubble.transform.position = transform.position + Vector3.down * 0.8f + Random.insideUnitSphere * 0.25f;
        bubble.transform.localScale = Vector3.one * Random.Range(0.12f, 0.25f);
        bubble.GetComponent<Renderer>().material.color = new Color(0.8f, 0.95f, 1f);
        bubble.AddComponent<FloatingBubble>().Init(0.6f, 0.5f);
    }
}
