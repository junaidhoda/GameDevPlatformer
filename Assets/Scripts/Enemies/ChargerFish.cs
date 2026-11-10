using UnityEngine;

// Swims back and forth. When the player gets close it stops, flashes red,
// then charges in a straight line at where the player was standing,
// before swimming back to its patrol. Dash into it to defeat it.
public class ChargerFish : MonoBehaviour
{
    [Header("Patrol")]
    public Vector3 patrolOffset = new Vector3(6f, 0f, 0f);
    public float patrolSpeed = 2f;

    [Header("Charge")]
    public float detectRange = 7f;
    public float windUpTime = 0.6f;
    public float chargeSpeed = 9f;
    public float chargeTime = 0.8f;
    public float restTime = 1.5f;
    public Color warningColour = new Color(1f, 0.2f, 0.2f);

    private enum State { Patrol, WindUp, Charge, Return }
    private State state = State.Patrol;

    private Vector3 patrolStart;
    private Vector3 chargeDirection;
    private Vector3 windUpPosition;
    private float stateEndTime;
    private float nextChargeTime;
    private Transform player;
    private Renderer body;
    private Color normalColour;

    void Start()
    {
        patrolStart = transform.position;

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null) player = health.transform;

        Transform bodyPart = transform.Find("Body");
        body = bodyPart != null ? bodyPart.GetComponent<Renderer>() : GetComponentInChildren<Renderer>();
        if (body != null) normalColour = body.material.color;
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: Patrol(); break;
            case State.WindUp: WindUp(); break;
            case State.Charge: Charge(); break;
            case State.Return: ReturnToPatrol(); break;
        }
    }

    // Where the fish should be on its back-and-forth route right now
    Vector3 PatrolPoint()
    {
        float distance = patrolOffset.magnitude;
        if (distance < 0.01f) return patrolStart;
        float t = Mathf.PingPong(Time.time * patrolSpeed / distance, 1f);
        return patrolStart + patrolOffset * t;
    }

    void Patrol()
    {
        Vector3 target = PatrolPoint();
        Face(target - transform.position);
        transform.position = target;

        if (player == null || Time.time < nextChargeTime) return;

        Vector3 toPlayer = Flat(player.position - transform.position);
        if (toPlayer.magnitude <= detectRange)
        {
            windUpPosition = transform.position;
            chargeDirection = toPlayer.normalized;
            Face(chargeDirection);
            state = State.WindUp;
            stateEndTime = Time.time + windUpTime;
        }
    }

    void WindUp()
    {
        // Flash and shake so the player can see it coming
        bool flashOn = Mathf.Repeat(Time.time * 10f, 1f) > 0.5f;
        if (body != null) body.material.color = flashOn ? warningColour : normalColour;
        transform.position = windUpPosition + Random.insideUnitSphere * 0.05f;

        if (Time.time >= stateEndTime)
        {
            transform.position = windUpPosition;
            if (body != null) body.material.color = warningColour;
            state = State.Charge;
            stateEndTime = Time.time + chargeTime;
        }
    }

    void Charge()
    {
        transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

        if (Time.time >= stateEndTime)
        {
            if (body != null) body.material.color = normalColour;
            state = State.Return;
        }
    }

    void ReturnToPatrol()
    {
        // Swim back to the moving patrol route, then carry on patrolling
        Vector3 home = PatrolPoint();
        Vector3 toHome = home - transform.position;
        Face(toHome);
        transform.position = Vector3.MoveTowards(transform.position, home, patrolSpeed * 3f * Time.deltaTime);

        if (toHome.magnitude < 0.2f)
        {
            state = State.Patrol;
            nextChargeTime = Time.time + restTime;
        }
    }

    void Face(Vector3 direction)
    {
        direction = Flat(direction);
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    static Vector3 Flat(Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }
}
