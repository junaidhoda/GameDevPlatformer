using UnityEngine;

// Fires arrows straight ahead (along its blue Z arrow) every few seconds
// while the player is nearby. Arrows hurt the player but can be dashed through.
public class ArrowTrap : MonoBehaviour
{
    public float fireInterval = 2f;
    public float startDelay = 0f;
    public float arrowSpeed = 10f;
    public float arrowLifetime = 4f;
    public int damage = 15;
    public float activationRange = 30f;
    public GameObject arrowPrefab;

    [Header("Warning")]
    [Tooltip("The nozzle glows for this long before each shot")]
    public float warningTime = 0.4f;
    public Color warningColour = new Color(1f, 0.85f, 0.2f);

    private Transform player;
    private float nextFire;
    private Renderer nozzle;
    private Color nozzleColour;

    void Start()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null) player = health.transform;

        Transform nozzlePart = transform.Find("Nozzle");
        if (nozzlePart != null)
        {
            nozzle = nozzlePart.GetComponent<Renderer>();
            if (nozzle != null) nozzleColour = nozzle.material.color;
        }

        nextFire = Time.time + startDelay;
    }

    bool PlayerInRange()
    {
        return player == null ||
            Vector3.Distance(player.position, transform.position) <= activationRange;
    }

    void Update()
    {
        // Glow just before firing so the player can time their run
        if (nozzle != null)
        {
            bool warn = nextFire - Time.time < warningTime && PlayerInRange();
            nozzle.material.color = warn ? warningColour : nozzleColour;
        }

        if (Time.time < nextFire) return;
        nextFire = Time.time + fireInterval;

        if (!PlayerInRange()) return;

        Fire();
    }

    void Fire()
    {
        Vector3 spawnPosition = transform.position + transform.forward * 0.8f;

        GameObject arrow = arrowPrefab != null
            ? Instantiate(arrowPrefab, spawnPosition, transform.rotation)
            : BuildArrow(spawnPosition);

        ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile == null) projectile = arrow.AddComponent<ArrowProjectile>();

        projectile.Init(transform.forward * arrowSpeed, damage, arrowLifetime, gameObject);
    }

    // Simple arrow made from basic shapes, used when no prefab is set
    GameObject BuildArrow(Vector3 position)
    {
        GameObject root = new GameObject("Arrow");
        root.transform.SetPositionAndRotation(position, transform.rotation);

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        DestroyImmediate(shaft.GetComponent<Collider>());
        shaft.transform.SetParent(root.transform, false);
        shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shaft.transform.localScale = new Vector3(0.07f, 0.4f, 0.07f);
        shaft.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);

        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DestroyImmediate(tip.GetComponent<Collider>());
        tip.transform.SetParent(root.transform, false);
        tip.transform.localPosition = new Vector3(0f, 0f, 0.45f);
        tip.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        tip.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
        tip.GetComponent<Renderer>().material.color = new Color(0.75f, 0.75f, 0.8f);

        SphereCollider hitbox = root.AddComponent<SphereCollider>();
        hitbox.isTrigger = true;
        hitbox.radius = 0.25f;

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        return root;
    }
}
