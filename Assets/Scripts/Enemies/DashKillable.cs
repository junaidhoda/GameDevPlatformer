using UnityEngine;

// Marks an enemy the player can defeat by dashing into it.
// PlayerDash calls Kill() when the dash hits it.
public class DashKillable : MonoBehaviour
{
    public Color popColour = new Color(0.75f, 0.95f, 1f);

    public void Kill()
    {
        Pop(transform.position, popColour);
        Destroy(gameObject);
    }

    // Small burst of bubbles where something was defeated
    public static void Pop(Vector3 position, Color colour)
    {
        GameObject pop = new GameObject("Pop");
        pop.transform.position = position;
        pop.AddComponent<PopEffect>().Init(colour);
    }
}
