using UnityEngine;

// A pile of sticky rubbish. Touching it gums up the diver's suit: for a few
// seconds they walk slower and can't jump as high (too low to climb a step).
// Needs a trigger collider.
public class StickyTrash : MonoBehaviour
{
    [Range(0.1f, 1f)] public float moveMultiplier = 0.45f;
    [Range(0.1f, 1f)] public float jumpMultiplier = 0.5f;
    public float stickTime = 3f;

    void OnTriggerEnter(Collider other) { Stick(other); }
    void OnTriggerStay(Collider other) { Stick(other); }

    void Stick(Collider other)
    {
        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement == null) return;

        movement.ApplySlow(moveMultiplier, jumpMultiplier, stickTime);
        HUDMessage.Show("STUCK IN STICKY TRASH!", 1f, new Color(0.95f, 0.8f, 0.35f));
    }
}
