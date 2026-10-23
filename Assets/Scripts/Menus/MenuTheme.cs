using TMPro;
using UnityEngine;

// The look shared by the in-game menus (Level Complete and Pause), taken from
// the Main Menu scene. Lives at Assets/Resources/MenuTheme.asset - select it to
// change the background, bubbles or sprites.
[CreateAssetMenu(fileName = "MenuTheme", menuName = "Menus/Menu Theme")]
public class MenuTheme : ScriptableObject
{
    [Header("Background")]
    public Sprite background;
    public Color backgroundTint = new Color(0.651f, 0.651f, 0.651f, 1f);

    [Header("Bubbles")]
    public GameObject bubblePrefab;
    public int bubbleCount = 30;
    public float bubbleMinSize = 20f;
    public float bubbleMaxSize = 55f;

    [Header("Panel and buttons")]
    public Sprite panelSprite;
    public Sprite buttonSprite;
    public TMP_FontAsset font;

    private static MenuTheme loaded;

    public static MenuTheme Get()
    {
        if (loaded == null) loaded = Resources.Load<MenuTheme>("MenuTheme");
        return loaded;
    }
}
