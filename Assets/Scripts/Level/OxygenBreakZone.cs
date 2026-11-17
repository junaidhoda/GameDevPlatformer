using UnityEngine;

// Invisible area near the end of Level 2. When the player walks in,
// their oxygen tank cracks and they have to use air bubbles to keep breathing.
// Needs a trigger collider.
public class OxygenBreakZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        OxygenSupply oxygen = other.GetComponent<OxygenSupply>();
        if (oxygen != null) oxygen.BreakTank();
    }
}
