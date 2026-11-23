using UnityEngine;
using UnityEngine.InputSystem;

// Quick burst of movement on the ground or in the air (Q or right mouse button).
// While dashing the player can't be hurt, and anything dangerous in the way
// (pufferfish, chargers, jellyfish, spitters, exploding fish, sea mines, arrows, bubbles) is destroyed.
[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 24f;
    [Tooltip("How far one dash goes")]
    public float dashDistance = 7.5f;
    [Tooltip("Seconds before you can dash again")]
    public float rechargeTime = 0.6f;
    [Tooltip("Pressing dash this long before it is ready still dashes when it is")]
    public float dashBufferTime = 0.2f;

    [Header("Combat")]
    public float hitRadius = 1.3f;
    public float safeTimeAfterDash = 0.2f;

    [Header("Look")]
    public Color dashColour = new Color(0.3f, 0.9f, 1f);

    public bool IsDashing { get; private set; }
    public bool IsInvulnerable => IsDashing || Time.time < safeUntil;
    public float CooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);
    public float CooldownLength => dashDistance / Mathf.Max(1f, dashSpeed) + rechargeTime;

    private CharacterController controller;
    private PlayerMovement movement;
    private Renderer bodyRenderer;
    private TrailRenderer trail;
    private Color normalColour;

    private Vector3 dashDirection;
    private Vector3 lastMoveDirection = Vector3.forward;
    private float dashEndTime;
    private float nextDashTime;
    private float safeUntil;
    private bool airDashUsed;
    private bool airDash;
    private float dashPressedTime = -999f;
    private bool wasReady = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();

        bodyRenderer = GetComponent<Renderer>();
        if (bodyRenderer != null) normalColour = bodyRenderer.material.color;

        Shader trailShader = Shader.Find("Sprites/Default");
        if (trailShader != null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.material = new Material(trailShader);
            trail.time = 0.2f;
            trail.startWidth = 0.8f;
            trail.endWidth = 0f;
            trail.startColor = dashColour;
            trail.endColor = new Color(dashColour.r, dashColour.g, dashColour.b, 0f);
            trail.emitting = false;
        }
    }

    // Use PlayerMovement's grounded state; controller.isGrounded is unreliable
    // while a moving platform is carrying the player.
    private bool Grounded => movement != null ? movement.IsGrounded : controller.isGrounded;

    void Update()
    {
        if (Time.timeScale == 0f) return; // paused / menu open

        Vector3 input = ReadMoveInput();
        if (input.sqrMagnitude > 0.01f) lastMoveDirection = input;

        if (Grounded) airDashUsed = false;

        bool canDash = !IsDashing
            && Time.time >= nextDashTime
            && (Grounded || !airDashUsed);

        // Remember dash presses for a moment, so pressing slightly too early still works
        if (DashPressed()) dashPressedTime = Time.time;
        bool wantsDash = Time.time - dashPressedTime <= dashBufferTime;

        if (canDash && wantsDash)
        {
            dashPressedTime = -999f;
            StartDash();
        }

        // Quick flash on the diver when the dash is ready again
        bool ready = Time.time >= nextDashTime && !IsDashing;
        if (ready && !wasReady) DashKillable.Pop(transform.position, dashColour);
        wasReady = ready;

        if (IsDashing)
        {
            // An air dash goes straight across instead of dropping, so gaps are easier to judge
            if (airDash && movement != null) movement.Launch(0f);

            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            DestroyThingsInPath();

            if (Time.time >= dashEndTime) EndDash();
        }
    }

    void StartDash()
    {
        IsDashing = true;
        dashDirection = lastMoveDirection.normalized;
        float duration = dashDistance / Mathf.Max(1f, dashSpeed);
        dashEndTime = Time.time + duration;
        nextDashTime = dashEndTime + rechargeTime;
        airDash = !Grounded;

        if (airDash)
        {
            airDashUsed = true;
            // Stop falling for a moment so an air dash can cross gaps
            if (movement != null) movement.Launch(0f);
        }

        if (bodyRenderer != null) bodyRenderer.material.color = dashColour;
        if (trail != null) trail.emitting = true;
    }

    void EndDash()
    {
        IsDashing = false;
        safeUntil = Time.time + safeTimeAfterDash;

        if (bodyRenderer != null) bodyRenderer.material.color = normalColour;
        if (trail != null) trail.emitting = false;
    }

    void DestroyThingsInPath()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, hitRadius, ~0, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;

            DashKillable enemy = hit.GetComponentInParent<DashKillable>();
            if (enemy != null) { enemy.Kill(); continue; }

            PufferfishAI fish = hit.GetComponentInParent<PufferfishAI>();
            if (fish != null)
            {
                DashKillable.Pop(fish.transform.position, new Color(1f, 0.85f, 0.4f));
                Destroy(fish.gameObject);
                continue;
            }

            SeaMineDamage mine = hit.GetComponentInParent<SeaMineDamage>();
            if (mine != null)
            {
                DashKillable.Pop(mine.transform.position, new Color(1f, 0.5f, 0.3f));
                Destroy(mine.gameObject);
                continue;
            }

            ArrowProjectile arrow = hit.GetComponentInParent<ArrowProjectile>();
            if (arrow != null)
            {
                DashKillable.Pop(arrow.transform.position, new Color(0.8f, 0.95f, 1f));
                Destroy(arrow.gameObject);
                continue;
            }

            ExplodingFish boomFish = hit.GetComponentInParent<ExplodingFish>();
            if (boomFish != null)
            {
                DashKillable.Pop(boomFish.transform.position, new Color(1f, 0.55f, 0.15f));
                Destroy(boomFish.gameObject);
                continue;
            }
        }
    }

    bool DashPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
        bool mouse = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        return key || mouse;
    }

    // Same keys and directions as PlayerMovement
    Vector3 ReadMoveInput()
    {
        Vector3 input = Vector3.zero;
        if (Keyboard.current == null) return input;

        if (Keyboard.current.wKey.isPressed) input.z += 1;
        if (Keyboard.current.sKey.isPressed) input.z -= 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;

        return input.normalized;
    }
}
