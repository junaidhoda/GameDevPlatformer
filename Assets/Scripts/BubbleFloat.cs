
using UnityEngine;

public class BubbleFloat : MonoBehaviour
{
    [Header("Vertical Movement")]
    public float minSpeed = 15f;
    public float maxSpeed = 90f;

    [Header("Side-to-Side Movement")]
    public float minSwayAmount = 5f;
    public float maxSwayAmount = 18f;

    public float minSwaySpeed = 0.15f;
    public float maxSwaySpeed = 0.35f;

    private RectTransform bubble;
    private RectTransform area;

    private float speed;
    private float swayAmount;
    private float swaySpeed;
    private float baseX;
    private float randomOffset;

    void Start()
    {
        bubble = GetComponent<RectTransform>();
        area = transform.parent.GetComponent<RectTransform>();

        // Random vertical speed
        speed = Random.Range(minSpeed, maxSpeed);

        // Random side-to-side movement
        swayAmount = Random.Range(
            minSwayAmount, maxSwayAmount
        );

        swaySpeed = Random.Range(
            minSwaySpeed, maxSwaySpeed
        );

        // Make each bubble sway at a different time
        randomOffset = Random.Range(0f, Mathf.PI * 2f);

        // Random starting horizontal position
        baseX = RandomX();

        // Random starting vertical position
        bubble.anchoredPosition = new Vector2(
            baseX,
            Random.Range(
                -area.rect.height / 2f,
                area.rect.height / 2f
            )
        );
    }

    void Update()
    {
        Vector2 position = bubble.anchoredPosition;

        // Move upwards
        position.y += speed * Time.unscaledDeltaTime; // keeps moving while paused

        // Gently sway from side to side
        float sway = Mathf.Sin(
            Time.unscaledTime * swaySpeed * Mathf.PI * 2f
            + randomOffset
        ) * swayAmount;

        position.x = baseX + sway;

        // Respawn when reaching the top
        if (position.y > area.rect.height / 2f + 40f)
        {
            position.y = -area.rect.height / 2f - 40f;

            baseX = RandomX();

            randomOffset = Random.Range(
                0f, Mathf.PI * 2f
            );
        }

        bubble.anchoredPosition = position;
    }

    float RandomX()
    {
        float limit = Mathf.Max(
            0f,
            area.rect.width / 2f - maxSwayAmount - 30f
        );

        return Random.Range(-limit, limit);
    }
}
