
using UnityEngine;
using TMPro;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Bubble Settings")]
    public GameObject bubblePrefab;

    [Range(1, 100)]
    public int bubbleCount = 30;

    [Header("Bubble Sizes")]
    public float minSize = 14f;
    public float maxSize = 55f;

    void Start()
    {
        if (bubblePrefab == null)
        {
            Debug.LogError("Bubble Prefab not assigned!");
            return;
        }

        for (int i = 0; i < bubbleCount; i++)
        {
            // Create a new bubble
            GameObject bubble = Instantiate(
                bubblePrefab,
                transform,
                false
            );

            bubble.name = "Bubble_" + (i + 1);

            // Keep bubbles behind menu buttons
            bubble.transform.SetSiblingIndex(1);

            // Randomise bubble size and transparency
            TextMeshProUGUI text =
                bubble.GetComponent<TextMeshProUGUI>();

            if (text != null)
            {
                text.fontSize = Random.Range(
                    minSize,
                    maxSize
                );

                Color color = text.color;

                color.a = Random.Range(0.2f, 0.45f);

                text.color = color;
            }
        }
    }
}
