using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SoftLightRay : MonoBehaviour
{
    [Range(0f, 1f)]
    public float opacity = 0.15f;

    private Texture2D texture;
    private Sprite gradientSprite;

    void Start()
    {
        int width = 64;
        int height = 256;

        texture = new Texture2D(
            width, height,
            TextureFormat.RGBA32, false
        );

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            float vertical = (float)y / (height - 1);
            float fadeBottom = Mathf.SmoothStep(
                0f, 1f, vertical
            );

            for (int x = 0; x < width; x++)
            {
                float horizontal = (float)x / (width - 1);

                float fadeSides = Mathf.Pow(
                    Mathf.Sin(horizontal * Mathf.PI), 2f
                );

                float alpha = fadeSides * fadeBottom * opacity;

                texture.SetPixel(
                    x, y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        texture.Apply();

        gradientSprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f)
        );

        Image image = GetComponent<Image>();
        image.sprite = gradientSprite;
        image.type = Image.Type.Simple;
        image.color = new Color(0.725f, 0.957f, 1f, 1f);
        image.raycastTarget = false;
    }

    void OnDestroy()
    {
        if (gradientSprite != null)
            Destroy(gradientSprite);

        if (texture != null)
            Destroy(texture);
    }
}
