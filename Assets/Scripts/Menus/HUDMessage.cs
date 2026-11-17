using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows a short message just below the middle of the screen, then fades it out.
// Call HUDMessage.Show("TEXT", seconds, colour) from anywhere; nothing needs
// to be placed in the scene.
public class HUDMessage : MonoBehaviour
{
    const float FadeTime = 0.5f;

    private static HUDMessage instance;

    private TextMeshProUGUI text;
    private float hideTime;

    public static void Show(string message, float duration, Color colour)
    {
        if (instance == null) instance = Create();

        instance.text.text = message;
        instance.text.color = colour;
        instance.hideTime = Time.time + duration;
    }

    static HUDMessage Create()
    {
        GameObject root = new GameObject("HUDMessage");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60; // above the HUD, below the menus (100)

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(root.transform, false);
        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -170f); // under LevelHUD's banner
        rect.sizeDelta = new Vector2(1400f, 200f);

        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        MenuTheme theme = MenuTheme.Get();
        if (theme != null && theme.font != null) tmp.font = theme.font;
        tmp.fontSize = 44;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 200);

        HUDMessage message = root.AddComponent<HUDMessage>();
        message.text = tmp;
        return message;
    }

    void Update()
    {
        text.alpha = Mathf.Clamp01((hideTime - Time.time) / FadeTime);
    }
}
