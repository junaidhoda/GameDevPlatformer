
using UnityEngine;

public class PufferfishAI : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float patrolRange = 8f;
    public int damage = 20;

    private Vector3 startingPosition;
    private Vector3 targetPosition;
    private float nextHitTime;

    void Start()
    {
        startingPosition = transform.position;
        ChooseNewTarget();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            ChooseNewTarget();
        }
    }

    void ChooseNewTarget()
    {
        targetPosition = startingPosition + new Vector3(
            Random.Range(-patrolRange, patrolRange),
            0,
            Random.Range(-patrolRange, patrolRange)
        );
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null && Time.time >= nextHitTime)
        {
            player.TakeDamage(damage);
            nextHitTime = Time.time + 1.5f;
        }
    }
}
