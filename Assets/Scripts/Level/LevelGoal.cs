using UnityEngine;

// Put this on the end-of-level platform. When the player lands on it
// (detected by PlayerPlatformContact) and has collected any treasure sitting
// on it, the game pauses and the Level Complete menu appears.
public class LevelGoal : MonoBehaviour
{
    [Header("Menu text")]
    public string titleTop = "LEVEL 1";
    public string titleBottom = "COMPLETE!";
    [TextArea] public string description = "You made it through the depths! Ready to dive deeper?";
    public string continueLabel = "CONTINUE TO LEVEL 2";

    [Tooltip("Scene loaded by the Continue button. Must be added to File > Build Settings.")]
    public string nextSceneName = "Level2";

    [Tooltip("Pause after the last goal treasure is picked up before the menu opens")]
    public float menuDelay = 0.75f;

    private bool playerOnGoal;
    private bool finished;
    private float finishTime = -1f;
    private TreasurePickup[] goalTreasures = new TreasurePickup[0];

    void Start()
    {
        // Any treasure sitting on top of this platform must be collected
        // before the level ends.
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds b = col.bounds;
        var found = new System.Collections.Generic.List<TreasurePickup>();
        foreach (TreasurePickup t in FindObjectsByType<TreasurePickup>(FindObjectsSortMode.None))
        {
            Vector3 p = t.transform.position;
            bool above = p.x >= b.min.x - 0.5f && p.x <= b.max.x + 0.5f
                      && p.z >= b.min.z - 0.5f && p.z <= b.max.z + 0.5f
                      && p.y >= b.max.y && p.y <= b.max.y + 4f;
            if (above) found.Add(t);
        }
        goalTreasures = found.ToArray();
    }

    // Called by PlayerPlatformContact every frame the player stands on the goal
    public void Reach()
    {
        if (playerOnGoal) return;
        playerOnGoal = true;

        // Stop the old win / lose checks now, so collecting the goal treasure
        // (often the last one) can't trigger "YOU WIN!" over the Level Complete menu.
        // (Done while the game is running, because its OnDisable resets timeScale.)
        GameEndManager endManager = FindFirstObjectByType<GameEndManager>();
        if (endManager != null) endManager.enabled = false;
    }

    void Update()
    {
        if (finished || !playerOnGoal) return;

        foreach (TreasurePickup t in goalTreasures)
            if (t != null && t.gameObject.activeInHierarchy) return; // still to collect

        if (finishTime < 0f) finishTime = Time.time + menuDelay;
        if (Time.time < finishTime) return;

        finished = true;
        LevelCompleteMenu.Show(titleTop, titleBottom, description, continueLabel, nextSceneName);
    }
}
