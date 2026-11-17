using System.Collections;
using UnityEngine;

// A fish that swims back and forth. When the player gets close it puffs up,
// flashes red and explodes, hurting the player and knocking them upwards if
// they're still in range. Dashing through it destroys it safely (PlayerDash).
// After exploding it comes back a few seconds later.
public class ExplodingFish : MonoBehaviour
{
    [Header("Swimming")]
    public Vector3 swimOffset = new Vector3(3f, 0f, 0f);
    public float swimSpeed = 1.5f;

    [Header("Explosion")]
    public float triggerRange = 2.8f;
    public float fuseTime = 0.9f;
    public float blastRadius = 3.2f;
    public int damage = 25;
    public float knockUpSpeed = 7f;
    public float respawnDelay = 5f;
    public Color flashColour = new Color(1f, 0.15f, 0.1f);

    private Transform player;
    private PlayerHealth health;
    private PlayerMovement movement;

    private Vector3 startPosition;
    private Vector3 normalScale;
    private Renderer[] renderers;
    private Color[] normalColours;
    private Collider[] colliders;
    private float swimTime;
    private bool busy;

    void Start()
    {
        health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            player = health.transform;
            movement = health.GetComponent<PlayerMovement>();
        }

        startPosition = transform.position;
        normalScale = transform.localScale;

        renderers = GetComponentsInChildren<Renderer>();
        normalColours = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            normalColours[i] = renderers[i].material.color;

        colliders = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        if (busy || player == null) return;

        // Swim back and forth
        float distance = swimOffset.magnitude;
        if (distance > 0.01f)
        {
            swimTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(swimTime * swimSpeed / distance, 1f));
            transform.position = startPosition + swimOffset * t;
        }

        if (Vector3.Distance(player.position, transform.position) <= triggerRange)
            StartCoroutine(ExplodeRoutine());
    }

    IEnumerator ExplodeRoutine()
    {
        busy = true;

        // Fuse: swell up and flash faster and faster
        float t = 0f;
        while (t < fuseTime)
        {
            t += Time.deltaTime;
            float k = t / fuseTime;
            transform.localScale = normalScale * (1f + 0.6f * k);
            bool flashOn = Mathf.Repeat(t * (5f + 12f * k), 1f) < 0.5f;
            SetFlash(flashOn);
            yield return null;
        }

        // Bang
        if (player != null && Vector3.Distance(player.position, transform.position) <= blastRadius)
        {
            PlayerDash dash = player.GetComponent<PlayerDash>();
            bool safe = dash != null && dash.IsInvulnerable;
            if (!safe)
            {
                health.TakeDamage(damage);
                if (movement != null) movement.Launch(knockUpSpeed);
            }
        }

        StartCoroutine(BlastEffect(transform.position));
        SetVisible(false);

        // Come back later
        yield return new WaitForSeconds(respawnDelay);

        swimTime = 0f;
        transform.position = startPosition;
        transform.localScale = normalScale;
        SetFlash(false);
        SetVisible(true);
        busy = false;
    }

    IEnumerator BlastEffect(Vector3 position)
    {
        GameObject blast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(blast.GetComponent<Collider>());
        blast.name = "FishBlast";
        blast.transform.position = position;

        Renderer r = blast.GetComponent<Renderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) r.material = new Material(shader);
        Color colour = new Color(1f, 0.6f, 0.2f, 0.6f);

        float t = 0f;
        const float duration = 0.35f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            blast.transform.localScale = Vector3.one * blastRadius * 2f * k;
            r.material.color = new Color(colour.r, colour.g, colour.b, colour.a * (1f - k));
            yield return null;
        }
        Destroy(blast);
    }

    void SetFlash(bool on)
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null)
                renderers[i].material.color = on ? flashColour : normalColours[i];
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers) if (r != null) r.enabled = visible;
        foreach (Collider c in colliders) if (c != null) c.enabled = visible;
    }
}
