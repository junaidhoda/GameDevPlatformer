using System.Collections;
using UnityEngine;

// A platform that shakes, changes colour and falls away shortly after
// the player stands on it, then comes back a few seconds later.
public class BreakingPlatform : MonoBehaviour
{
    public float breakDelay = 0.7f;
    public float fallTime = 1f;
    public float respawnDelay = 4f;
    public float shakeAmount = 0.06f;
    public Color warningColour = new Color(1f, 0.3f, 0.15f);

    private Vector3 startPosition;
    private Renderer rend;
    private Collider col;
    private Color startColour;
    private bool breaking;

    void Awake()
    {
        startPosition = transform.position;
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        if (rend != null) startColour = rend.material.color;
    }

    public void StepOn()
    {
        if (!breaking) StartCoroutine(BreakRoutine());
    }

    IEnumerator BreakRoutine()
    {
        breaking = true;

        // Shake and turn red as a warning
        float t = 0f;
        while (t < breakDelay)
        {
            t += Time.deltaTime;
            transform.position = startPosition + new Vector3(
                Random.Range(-shakeAmount, shakeAmount), 0f,
                Random.Range(-shakeAmount, shakeAmount));
            if (rend != null)
                rend.material.color = Color.Lerp(startColour, warningColour, t / breakDelay);
            yield return null;
        }

        // Fall away
        transform.position = startPosition;
        if (col != null) col.enabled = false;

        float fall = 0f;
        while (fall < fallTime)
        {
            fall += Time.deltaTime;
            transform.position += Vector3.down * 8f * Time.deltaTime;
            yield return null;
        }

        if (rend != null) rend.enabled = false;

        // Come back
        yield return new WaitForSeconds(respawnDelay);

        transform.position = startPosition;
        if (rend != null)
        {
            rend.material.color = startColour;
            rend.enabled = true;
        }
        if (col != null) col.enabled = true;
        breaking = false;
    }
}
