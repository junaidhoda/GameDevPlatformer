
using UnityEngine;
using TMPro;

public class TreasureManager : MonoBehaviour
{
    public TextMeshProUGUI counterText;

    private int collected = 0;
    private int total = 12;

    public bool AllTreasuresCollected()
    {
        return collected >= total;
    }

    void Start()
    {
        // Count every treasure in the scene, so adding levels updates the total
        int found = FindObjectsByType<TreasurePickup>(FindObjectsSortMode.None).Length;
        if (found > 0) total = found;

        UpdateCounter();
    }

    public void CollectTreasure()
    {
        collected++;
        UpdateCounter();

        if (collected == total)
        {
            Debug.Log("All treasures collected!");
        }
    }

    void UpdateCounter()
    {
        counterText.text = "Treasure: " + collected + "/" + total;
    }
}
