
using UnityEngine;
using TMPro;

public class TreasureManager : MonoBehaviour
{
    public TextMeshProUGUI counterText;

    private int collected = 0;
    private int total = 5;

    void Start()
    {
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
