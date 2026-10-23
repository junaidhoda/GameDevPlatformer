using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Builds a full-screen menu in code, styled like the Main Menu scene:
// sunken-ruins background, floating bubbles, dark-teal panel,
// gold + cyan two-line title, teal buttons and a dark-red Quit button.
public static class GameMenuUI
{
    public enum ButtonStyle { Primary, Danger }

    // Colours copied from MainMenu.unity
    static readonly Color PanelColour = new Color(0.0275f, 0.1216f, 0.1882f, 0.7255f);
    static readonly Color TitleGold = new Color(0.9608f, 0.7765f, 0.3647f, 1f);
    static readonly Color TitleCyan = new Color(0.498f, 0.8627f, 1f, 1f);
    static readonly Color DescriptionColour = new Color(0.7843f, 0.9137f, 0.9451f, 1f);

    const float PanelWidth = 480f;
    const float ButtonWidth = 290f;
    const float ButtonHeight = 70f;
    const float ButtonGap = 90f;

    public struct ButtonSpec
    {
        public string label;
        public UnityAction onClick;
        public ButtonStyle style;
        public bool interactable;

        public ButtonSpec(string label, UnityAction onClick, ButtonStyle style = ButtonStyle.Primary, bool interactable = true)
        {
            this.label = label;
            this.onClick = onClick;
            this.style = style;
            this.interactable = interactable;
        }
    }

    // Returns the root object; destroy it to close the menu.
    public static GameObject Build(string name, string titleTop, string titleBottom,
        string description, params ButtonSpec[] buttons)
    {
        MenuTheme theme = MenuTheme.Get();
        if (theme == null)
            Debug.LogWarning("MenuTheme not found in Assets/Resources - menu will use plain colours.");

        GameObject root = new GameObject(name);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // above the HUD

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        root.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        // Background image (same picture and tint as the main menu)
        RectTransform background = NewRect("BackgroundPanel", root.transform);
        Stretch(background);
        Image bgImage = background.gameObject.AddComponent<Image>();
        if (theme != null && theme.background != null)
        {
            bgImage.sprite = theme.background;
            bgImage.color = theme.backgroundTint;
        }
        else
        {
            bgImage.color = new Color(0.02f, 0.1f, 0.18f, 1f);
        }

        // Placeholder at index 0 so the spawned bubbles slot in behind the panel
        Stretch(NewRect("Decor", background));

        // Panel size grows with the number of buttons
        bool hasDescription = !string.IsNullOrEmpty(description);
        float firstButton = hasDescription ? 380f : 295f;   // distance from panel top
        float panelHeight = firstButton + (buttons.Length - 1) * ButtonGap + ButtonHeight / 2f + 60f;

        RectTransform panel = NewRect("Menu Panel", background);
        Centre(panel, 0f, new Vector2(PanelWidth, panelHeight));
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColour;
        if (theme != null && theme.panelSprite != null)
        {
            panelImage.sprite = theme.panelSprite;
            panelImage.type = Image.Type.Sliced;
        }

        float top = panelHeight / 2f;

        TextMeshProUGUI t1 = AddText(panel, "TitleTop", titleTop, 58, TitleGold, FontStyles.Bold, theme);
        Centre(t1.rectTransform, top - 110f, new Vector2(450, 90));

        TextMeshProUGUI t2 = AddText(panel, "TitleBottom", titleBottom, 56, TitleCyan, FontStyles.Bold, theme);
        Centre(t2.rectTransform, top - 175f, new Vector2(450, 80));

        if (hasDescription)
        {
            TextMeshProUGUI d = AddText(panel, "DescriptionText", description, 22, DescriptionColour, FontStyles.Normal, theme);
            Centre(d.rectTransform, top - 260f, new Vector2(420, 90));
        }

        Button firstSelectable = null;
        for (int i = 0; i < buttons.Length; i++)
        {
            Button b = AddButton(panel, buttons[i], theme);
            Centre((RectTransform)b.transform, top - firstButton - i * ButtonGap, new Vector2(ButtonWidth, ButtonHeight));
            if (firstSelectable == null && b.interactable) firstSelectable = b;
        }

        // Bubbles, using the same spawner and prefab as the main menu
        if (theme != null && theme.bubblePrefab != null)
        {
            BubbleSpawner spawner = background.gameObject.AddComponent<BubbleSpawner>();
            spawner.bubblePrefab = theme.bubblePrefab;
            spawner.bubbleCount = theme.bubbleCount;
            spawner.minSize = theme.bubbleMinSize;
            spawner.maxSize = theme.bubbleMaxSize;
        }

        if (EventSystem.current != null && firstSelectable != null)
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);

        return root;
    }

    // ---------- helpers ----------

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static void Centre(RectTransform rect, float y, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = size;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, float size,
        Color colour, FontStyles style, MenuTheme theme)
    {
        RectTransform rect = NewRect(name, parent);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (theme != null && theme.font != null) tmp.font = theme.font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = colour;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button AddButton(Transform parent, ButtonSpec spec, MenuTheme theme)
    {
        RectTransform rect = NewRect(spec.label + " Button", parent);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = Color.white;
        if (theme != null && theme.buttonSprite != null)
        {
            image.sprite = theme.buttonSprite;
            image.type = Image.Type.Sliced;
        }

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = spec.interactable;
        if (spec.onClick != null) button.onClick.AddListener(spec.onClick);

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectDistance = new Vector2(2f, -2f);

        ColorBlock c = button.colors;
        c.colorMultiplier = 1f;
        if (spec.style == ButtonStyle.Danger)
        {
            // Matches the main menu's QUIT button
            c.normalColor = new Color(0.1686f, 0.102f, 0.1647f, 1f);
            c.highlightedColor = new Color(0.5373f, 0.1647f, 0.2627f, 1f);
            c.pressedColor = new Color(0.6471f, 0.0627f, 0.1961f, 1f);
            c.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.502f);
            c.fadeDuration = 0.1f;
            outline.effectColor = new Color(0.8902f, 0.0314f, 0.2196f, 0.5f);
            outline.useGraphicAlpha = true;
        }
        else
        {
            // Matches the main menu's PLAY button
            c.normalColor = new Color(0.0314f, 0.498f, 0.6235f, 1f);
            c.highlightedColor = new Color(0.1412f, 0.7882f, 0.9098f, 1f);
            c.pressedColor = new Color(0.0196f, 0.3843f, 0.4863f, 1f);
            c.disabledColor = new Color(0.4f, 0.4902f, 0.5176f, 0.502f);
            c.fadeDuration = 0.15f;
            outline.effectColor = new Color(0.2824f, 0.898f, 1f, 0.5f);
            outline.useGraphicAlpha = false;
        }
        c.selectedColor = c.highlightedColor;
        button.colors = c;

        TextMeshProUGUI label = AddText(rect, "Text (TMP)", spec.label,
            spec.style == ButtonStyle.Danger ? 28 : 32, Color.white, FontStyles.Bold, theme);
        Stretch(label.rectTransform);
        if (!spec.interactable) label.color = new Color(1f, 1f, 1f, 0.6f);

        return button;
    }
}
