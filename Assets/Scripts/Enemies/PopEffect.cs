using UnityEngine;

// A handful of little spheres that fly outwards and shrink away.
// Created by DashKillable.Pop, removes itself when finished.
public class PopEffect : MonoBehaviour
{
    public float lifetime = 0.5f;
    public int pieceCount = 8;

    private Transform[] pieces;
    private Vector3[] velocities;
    private float startTime;

    public void Init(Color colour)
    {
        pieces = new Transform[pieceCount];
        velocities = new Vector3[pieceCount];

        for (int i = 0; i < pieceCount; i++)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(piece.GetComponent<Collider>());
            piece.transform.SetParent(transform, false);
            piece.transform.localScale = Vector3.one * Random.Range(0.15f, 0.3f);
            piece.GetComponent<Renderer>().material.color = colour;

            pieces[i] = piece.transform;
            velocities[i] = Random.onUnitSphere * Random.Range(3f, 6f);
        }

        startTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (pieces == null) return;

        float t = (Time.time - startTime) / lifetime;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            pieces[i].localPosition += velocities[i] * Time.deltaTime;
            pieces[i].localScale = Vector3.Lerp(pieces[i].localScale, Vector3.zero, t);
        }
    }
}
