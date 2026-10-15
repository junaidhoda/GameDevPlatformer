
using UnityEngine;

public class TreasurePickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null)
        {
            TreasureManager manager = FindObjectOfType<TreasureManager>();

            if (manager != null)
            {
                manager.CollectTreasure();
                gameObject.SetActive(false);
            }
        }
    }
}
