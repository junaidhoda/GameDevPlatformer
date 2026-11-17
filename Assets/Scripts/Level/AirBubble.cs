using System.Collections;
using UnityEngine;

// A floating bubble of air. Once the oxygen tank has broken, touching it
// refills some oxygen. It pops, then comes back a few seconds later.
// Needs a trigger collider.
public class AirBubble : MonoBehaviour
{
    public float oxygenAmount = 5f;
    public float respawnDelay = 4f;

    [Header("Look")]
    public float bobHeight = 0.25f;
    public float bobSpeed = 1.5f;
    public float pulseAmount = 0.08f;

    private Vector3 startPosition;
    private Vector3 normalScale;
    private Renderer[] renderers;
    private Collider col;
    private float offset;
    private bool popped;

    void Start()
    {
        startPosition = transform.localPosition;
        normalScale = transform.localScale;
        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
        offset = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (popped) return;

        float t = Time.time * bobSpeed + offset;
        transform.localPosition = startPosition + Vector3.up * Mathf.Sin(t) * bobHeight;
        transform.localScale = normalScale * (1f + Mathf.Sin(t * 2.3f) * pulseAmount);
    }

    void OnTriggerEnter(Collider other)
    {
        if (popped) return;

        OxygenSupply oxygen = other.GetComponent<OxygenSupply>();
        if (oxygen == null || !oxygen.TankBroken) return;

        oxygen.AddOxygen(oxygenAmount);
        StartCoroutine(PopRoutine());
    }

    IEnumerator PopRoutine()
    {
        popped = true;

        // Quick pop: swell and vanish
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            transform.localScale = normalScale * (1f + t * 4f);
            yield return null;
        }

        foreach (Renderer r in renderers) r.enabled = false;
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.localScale = normalScale;
        foreach (Renderer r in renderers) r.enabled = true;
        if (col != null) col.enabled = true;
        popped = false;
    }
}
