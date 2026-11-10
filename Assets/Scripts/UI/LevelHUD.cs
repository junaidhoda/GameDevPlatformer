using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Extra on-screen info, added automatically to any scene with a player:
// - the current level name under the timer
// - a big banner when a new level starts (with any bonus time)
// - a dash meter at the bottom that fills up when the dash is ready again
public class LevelHUD : MonoBehaviour
{
    static readonly Color Gold = new Color(0.9608f, 0.7765f, 0.3647f, 1f);
    static readonly Color Cyan = new Color(0.498f, 0.8627f, 1f, 1f);
    static readonly Color Pale = new Color(0.7843f, 0.9137f, 0.9451f, 1f);

    private static LevelHUD instance;

    private TextMeshProUGUI levelLabel;
    private TextMeshProUGUI bannerTitle;
    private TextMeshProUGUI bannerSubtitle;
    private CanvasGroup bannerGroup;
    private Image dashFill;
    private TextMeshProUGUI dashLabel;
    private PlayerDash dash;
    private Coroutine bannerRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (FindFirstObjectByType<PlayerMovement>() != null && FindFirstObjectByType<LevelHUD>() == null)
                new GameObject("LevelHUD").AddComponent<LevelHUD>();
        };
    }

    public static LevelHUD Get()
    {
        if (instance == null) instance = FindFirstObjectByType<LevelHUD>();
        return instance;
    }

    void Awake()
    {
        instance = this;
        Build();
    }

    void Start()
    {
        dash = FindFirstObjectByType<PlayerDash>();
        if (dash == null) dashFill.transform.parent.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dash == null) return;

        float cooldown = Mathf.Max(0.01f, dash.dashCooldown);
        float ready = 1f - Mathf.Clamp01(dash.CooldownRemaining / cooldown);
        dashFill.rectTransform.anchorMax = new Vector2(ready, 1f);
        dashFill.color = ready >= 1f ? Cyan : new Color(Cyan.r, Cyan.g, Cyan.b, 0.45f);
        dashLabel.text = ready >= 1f ? "DASH READY  (Q)" : "DASH";
    }

    public void SetLevelName(string text)
    {
        levelLabel.text = text;
    }

    public void ShowBanner(string title, string subtitle, float bonusSeconds)
    {
        SetLevelName(title + ": " + subtitle);

        bannerTitle.text = title;
        bannerSubtitle.text = bonusSeconds > 0f
            ? subtitle + "\n<size=60%>+" + Mathf.RoundToInt(bonusSeconds) + " seconds</size>"
            : subtitle;

        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(FadeBanner());
    }

    IEnumerator FadeBanner()
    {
        float fadeIn = 0.4f, hold = 2.2f, fadeOut = 0.8f;

        for (float t = 0f; t < fadeIn; t += Time.unscaledDeltaTime)
        {
            bannerGroup.alpha = t / fadeIn;
            yield return null;
        }
        bannerGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(hold);

        for (float t = 0f; t < fadeOut; t += Time.unscaledDeltaTime)
        {
            bannerGroup.alpha = 1f - t / fadeOut;
            yield return null;
        }
        bannerGroup.alpha = 0f;
    }

    // ---------- Building the UI ----------

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5; // under the pause / game over menus

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Level name, just under the timer at the top
        levelLabel = AddText(transform, "LevelName", "", 26, Pale, FontStyles.Bold);
        Place(levelLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(700f, 40f));

        // Big banner in the middle of the screen
        GameObject banner = new GameObject("LevelBanner", typeof(RectTransform));
        banner.transform.SetParent(transform, false);
        Place((RectTransform)banner.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1200f, 260f));
        bannerGroup = banner.AddComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;

        bannerTitle = AddText(banner.transform, "Title", "", 80, Gold, FontStyles.Bold);
        Place(bannerTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(1200f, 100f));
        bannerSubtitle = AddText(banner.transform, "Subtitle", "", 52, Cyan, FontStyles.Bold);
        Place(bannerSubtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(1200f, 140f));

        // Dash meter at the bottom middle
        GameObject meter = new GameObject("DashMeter", typeof(RectTransform));
        meter.transform.SetParent(transform, false);
        Place((RectTransform)meter.transform, new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(260f, 18f));
        Image back = meter.AddComponent<Image>();
        back.color = new Color(0f, 0f, 0f, 0.45f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(meter.transform, false);
        RectTransform fillRect = (RectTransform)fill.transform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        dashFill = fill.AddComponent<Image>();
        dashFill.color = Cyan;

        dashLabel = AddText(meter.transform, "Label", "DASH", 20, Pale, FontStyles.Bold);
        Place(dashLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 18f), new Vector2(300f, 30f));
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, float size, Color colour, FontStyles style)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        MenuTheme theme = MenuTheme.Get();
        if (theme != null && theme.font != null) tmp.font = theme.font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = colour;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 200);
        return tmp;
    }

    static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
