using UnityEngine;
using UnityEngine.SceneManagement;

// Simple visual upgrades from the playtest feedback ("add the character and more visual design"):
// - finishes off the diver: a glass visor on the helmet and a pair of flippers
// - breathing bubbles that rise from the helmet every so often
// - underwater fog so the world fades into blue in the distance
// Added automatically to the player when a level loads, so no scene changes are needed.
public class DiverLook : MonoBehaviour
{
    public float bubbleInterval = 0.9f;
    public Color visorColour = new Color(0.15f, 0.35f, 0.5f);
    public Color flipperColour = new Color(1f, 0.55f, 0.15f);

    private float nextBubble;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player == null) return;

            if (player.GetComponent<DiverLook>() == null)
                player.gameObject.AddComponent<DiverLook>();

            AddUnderwaterFog(scene.name);
        };
    }

    void Start()
    {
        if (transform.Find("DiverVisor") != null) return; // already dressed

        // Visor on the front of the helmet (the oxygen tank is on the back)
        Part("DiverVisor", PrimitiveType.Sphere, new Vector3(0f, 0.92f, 0.24f), new Vector3(0.45f, 0.32f, 0.25f), visorColour);

        // Two flippers at the feet, pointing forwards
        Part("FlipperLeft", PrimitiveType.Cube, new Vector3(-0.2f, -0.97f, 0.28f), new Vector3(0.26f, 0.06f, 0.55f), flipperColour);
        Part("FlipperRight", PrimitiveType.Cube, new Vector3(0.2f, -0.97f, 0.28f), new Vector3(0.26f, 0.06f, 0.55f), flipperColour);

        nextBubble = Time.time + Random.Range(0.2f, bubbleInterval);
    }

    void Update()
    {
        if (Time.timeScale == 0f || Time.time < nextBubble) return;
        nextBubble = Time.time + bubbleInterval * Random.Range(0.7f, 1.3f);

        // Two or three little breathing bubbles from the top of the helmet
        int count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(bubble.GetComponent<Collider>());
            bubble.name = "BreathBubble";
            bubble.transform.position = transform.position + transform.rotation * new Vector3(0f, 1.25f, 0.1f)
                                        + Random.insideUnitSphere * 0.1f + Vector3.up * i * 0.15f;
            bubble.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
            bubble.GetComponent<Renderer>().material.color = new Color(0.85f, 0.97f, 1f);
            bubble.AddComponent<FloatingBubble>().Init(1.2f, 1.6f);
        }
    }

    void Part(string partName, PrimitiveType shape, Vector3 position, Vector3 scale, Color colour)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        Destroy(part.GetComponent<Collider>());
        part.name = partName;
        part.transform.SetParent(transform, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().material.color = colour;
    }

    // Blue fog so far-away platforms fade into the water
    static void AddUnderwaterFog(string sceneName)
    {
        if (RenderSettings.fog) return; // the scene already sets its own fog

        bool deep = sceneName == "Level4";
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = deep ? new Color(0.02f, 0.08f, 0.14f) : new Color(0.16f, 0.42f, 0.52f);
        RenderSettings.fogStartDistance = deep ? 20f : 35f;
        RenderSettings.fogEndDistance = deep ? 75f : 110f;
    }
}
