using UnityEngine;

// Stays in place, turns to face the player and spits slow bubbles at them.
// Bubbles hurt the player but pop if the player dashes through them.
// Dash into the fish itself to defeat it.
public class SpitterFish : MonoBehaviour
{
    public float range = 14f;
    public float fireInterval = 2.4f;
    public float startDelay = 0.5f;
    public float bubbleSpeed = 6f;
    public float bubbleLifetime = 4f;
    public int damage = 15;
    public float warningTime = 0.4f;
    public Color bubbleColour = new Color(0.55f, 0.85f, 1f);

    private Transform player;
    private float nextFire;
    private Vector3 normalScale;

    void Start()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null) player = health.transform;

        normalScale = transform.localScale;
        nextFire = Time.time + startDelay;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        bool inRange = toPlayer.magnitude <= range;

        // Turn to face the player (left and right only)
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (inRange && flat.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 5f * Time.deltaTime);
        }

        // Puff up just before spitting so the player can react
        float timeLeft = nextFire - Time.time;
        float puff = inRange && timeLeft < warningTime ? 1.25f : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale, normalScale * puff, 12f * Time.deltaTime);

        if (Time.time < nextFire) return;
        nextFire = Time.time + fireInterval;

        if (inRange) Spit(toPlayer.normalized);
    }

    void Spit(Vector3 direction)
    {
        Vector3 spawnPosition = transform.position + direction * 0.9f;

        GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bubble.name = "Bubble";
        bubble.transform.position = spawnPosition;
        bubble.transform.localScale = Vector3.one * 0.5f;
        bubble.GetComponent<Renderer>().material.color = bubbleColour;

        SphereCollider hitbox = bubble.GetComponent<SphereCollider>();
        hitbox.isTrigger = true;

        Rigidbody rb = bubble.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Reuses the arrow logic: damage on hit, pops on anything solid, dash destroys it
        ArrowProjectile projectile = bubble.AddComponent<ArrowProjectile>();
        projectile.Init(direction * bubbleSpeed, damage, bubbleLifetime, gameObject);
    }
}
