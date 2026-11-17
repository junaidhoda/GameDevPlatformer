using UnityEngine;
using UnityEngine.UI;
using TMPro;

// The diver's oxygen. While the tank is fine nothing happens and no bar is shown.
// Once it breaks (see OxygenBreakZone) the oxygen drains and a bar appears at
// the top of the screen; air bubbles (AirBubble) top it up. Running out sends
// the player back to the last checkpoint with some damage.
[RequireComponent(typeof(CharacterController))]
public class OxygenSupply : MonoBehaviour
{
    [Header("Oxygen (seconds of air)")]
    public float maxOxygen = 8f;
    public float drainPerSecond = 1f;
    public int outOfAirDamage = 20;
    [Range(0f, 1f)] public float lowWarning = 0.35f;

    [Header("Tank model")]
    [Tooltip("Name of the tank model on the player, tinted red when it breaks")]
    public string tankModelName = "OxygenTank";
    public Color brokenTankColour = new Color(0.8f, 0.15f, 0.1f);

    public bool TankBroken { get; private set; }
    public float Oxygen { get; private set; }

    static readonly Color BarColour = new Color(0.498f, 0.8627f, 1f, 1f);
    static readonly Color LowColour = new Color(1f, 0.25f, 0.2f, 1f);
    const float BarWidth = 420f;

    private CharacterController controller;
    private PlayerHealth health;
    private GameObject hud;
    private RectTransform fill;
    private Image fillImage;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth>();
        Oxygen = maxOxygen;
    }

    public void BreakTank()
    {
        if (TankBroken) return;
        TankBroken = true;
        Oxygen = maxOxygen;

        BuildHud();
        TintTank();
        HUDMessage.Show("YOUR OXYGEN TANK CRACKED!\nGrab the air bubbles to keep breathing", 3.5f, BarColour);
    }

    public void AddOxygen(float amount)
    {
        Oxygen = Mathf.Min(maxOxygen, Oxygen + amount);
    }

    public void Refill()
    {
        Oxygen = maxOxygen;
    }

    void Update()
    {
        if (!TankBroken || Time.timeScale == 0f) return;

        Oxygen -= drainPerSecond * Time.deltaTime;
        if (Oxygen <= 0f) OutOfAir();

        UpdateHud();
    }

    void OutOfAir()
    {
        Refill();
        KillZone.Respawn(controller);
        if (health != null) health.TakeDamage(outOfAirDamage);
        HUDMessage.Show("OUT OF AIR!", 2f, LowColour);
    }

    // ---------- Look ----------

    void TintTank()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name != tankModelName) continue;
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
                r.material.color = brokenTankColour;
        }
    }

    void BuildHud()
    {
        if (hud != null) return;

        hud = new GameObject("OxygenHUD");
        Canvas canvas = hud.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // below the menus (100)

        CanvasScaler scaler = hud.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        // Dark frame at the top centre of the screen
        RectTransform frame = NewRect("OxygenBar", hud.transform);
        frame.anchorMin = frame.anchorMax = new Vector2(0.5f, 1f);
        frame.pivot = new Vector2(0.5f, 1f);
        frame.anchoredPosition = new Vector2(0f, -140f); // below the timer and level name
        frame.sizeDelta = new Vector2(BarWidth + 8f, 38f);
        frame.gameObject.AddComponent<Image>().color = new Color(0.0275f, 0.1216f, 0.1882f, 0.85f);

        // Fill grows from the left
        fill = NewRect("Fill", frame);
        fill.anchorMin = new Vector2(0f, 0f);
        fill.anchorMax = new Vector2(0f, 1f);
        fill.pivot = new Vector2(0f, 0.5f);
        fill.anchoredPosition = new Vector2(4f, 0f);
        fill.sizeDelta = new Vector2(BarWidth, -8f);
        fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = BarColour;

        RectTransform labelRect = NewRect("Label", frame);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        MenuTheme theme = MenuTheme.Get();
        if (theme != null && theme.font != null) label.font = theme.font;
        label.text = "OXYGEN";
        label.fontSize = 22;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.02f, 0.1f, 0.18f, 1f);
        label.raycastTarget = false;

        UpdateHud();
    }

    void UpdateHud()
    {
        if (fill == null) return;

        float fraction = Mathf.Clamp01(Oxygen / maxOxygen);
        fill.sizeDelta = new Vector2(BarWidth * fraction, -8f);

        if (fraction < lowWarning)
        {
            bool flashOn = Mathf.Repeat(Time.time * 4f, 1f) < 0.5f;
            fillImage.color = flashOn ? LowColour : Color.Lerp(LowColour, BarColour, 0.4f);
        }
        else
        {
            fillImage.color = BarColour;
        }
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }
}
