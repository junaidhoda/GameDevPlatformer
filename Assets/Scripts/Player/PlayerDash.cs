using UnityEngine;
using UnityEngine.InputSystem;

// Quick burst of movement on the ground or in the air (Q or right mouse button).
// While dashing the player can't be hurt, and anything dangerous in the way
// (pufferfish, sea mines, barrels, arrows) is destroyed.
[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 24f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.8f;

    [Header("Combat")]
    public float hitRadius = 1.3f;
    public float safeTimeAfterDash = 0.2f;

    [Header("Look")]
    public Color dashColour = new Color(0.3f, 0.9f, 1f);

    public bool IsDashing { get; private set; }
    public bool IsInvulnerable => IsDashing || Time.time < safeUntil;
    public float CooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);

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

        if (canDash && DashPressed()) StartDash();

        if (IsDashing)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            DestroyThingsInPath();

            if (Time.time >= dashEndTime) EndDash();
        }
    }

    void StartDash()
    {
        IsDashing = true;
        dashDirection = lastMoveDirection.normalized;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        if (!Grounded)
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

            PufferfishAI fish = hit.GetComponentInParent<PufferfishAI>();
            if (fish != null) { Destroy(fish.gameObject); continue; }

            SeaMineDamage mine = hit.GetComponentInParent<SeaMineDamage>();
            if (mine != null) { Destroy(mine.gameObject); continue; }

            ArrowProjectile arrow = hit.GetComponentInParent<ArrowProjectile>();
            if (arrow != null) { Destroy(arrow.gameObject); continue; }
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
