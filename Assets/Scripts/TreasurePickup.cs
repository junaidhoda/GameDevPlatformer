
using UnityEngine;

public class TreasurePickup : MonoBehaviour
{
    // Health given back when this treasure is picked up
    public int healAmount = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null)
        {
            TreasureManager manager = FindObjectOfType<TreasureManager>();

            if (manager != null)
            {
                manager.CollectTreasure();

                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null) health.Heal(healAmount);

                gameObject.SetActive(false);
            }
        }
    }
}
