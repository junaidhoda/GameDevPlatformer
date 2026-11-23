using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -15f;

    [Tooltip("Grace period (seconds) after leaving the ground where a jump still works")]
    public float coyoteTime = 0.1f;

    [Header("Playtest changes")]
    [Tooltip("Hold Left Shift to move this many times faster")]
    public float sprintMultiplier = 1.6f;
    [Tooltip("Pressing jump this long before landing still jumps when you land")]
    public float jumpBufferTime = 0.15f;
    [Tooltip("Letting go of jump early cuts the jump short, for small hops")]
    public float shortHopMultiplier = 0.5f;
    [Tooltip("How quickly the diver turns to face where they are moving")]
    public float turnSpeed = 12f;

    private CharacterController controller;
    private float verticalSpeed;
    private float lastGroundedTime = -999f;
    private float jumpPressedTime = -999f;
    private bool wasGrounded = true;
    private float fallSpeed;
    private bool risingFromOwnJump; // jump pads shouldn't be cut short

    // Grounded state from THIS script's own Move call.
    // controller.isGrounded can't be trusted on its own, because other scripts
    // (e.g. PlayerPlatformContact carrying the player on a moving platform)
    // call controller.Move afterwards and overwrite it.
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }

    // Slippery platforms (SlipperyPlatform): speed changes slowly so you slide
    private Vector3 horizontalVelocity;
    private int slipperyFrame = -10;
    private float slipperyGrip = 5f;

    // Sticky trash (StickyTrash): slower and lower jumps for a while
    private float slowUntil;
    private float slowMoveMultiplier = 1f;
    private float slowJumpMultiplier = 1f;
    public bool IsSlowed => Time.time < slowUntil;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // paused / menu open

        Vector2 input = Vector2.zero;
        Keyboard keys = Keyboard.current;

        if (keys != null)
        {
            if (keys.wKey.isPressed) input.y += 1;
            if (keys.sKey.isPressed) input.y -= 1;
            if (keys.aKey.isPressed) input.x -= 1;
            if (keys.dKey.isPressed) input.x += 1;
        }

        Vector3 movement = new Vector3(input.x, 0, input.y).normalized;

        if (IsGrounded && verticalSpeed < 0)
            verticalSpeed = -2f;

        // Remember jump presses for a moment, so pressing just before landing still works
        if (keys != null && keys.spaceKey.wasPressedThisFrame)
            jumpPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantsJump = Time.time - jumpPressedTime <= jumpBufferTime;

        if (wantsJump && canJump)
        {
            float jumpScale = IsSlowed ? slowJumpMultiplier : 1f;
            verticalSpeed = Mathf.Sqrt(jumpHeight * jumpScale * -2f * gravity);
            lastGroundedTime = -999f; // prevent a double jump within the grace window
            jumpPressedTime = -999f;
            risingFromOwnJump = true;
        }

        // Let go of jump early for a smaller hop
        if (keys != null && keys.spaceKey.wasReleasedThisFrame && verticalSpeed > 0f && risingFromOwnJump)
        {
            verticalSpeed *= shortHopMultiplier;
            risingFromOwnJump = false;
        }
        if (verticalSpeed <= 0f) risingFromOwnJump = false;

        verticalSpeed += gravity * Time.deltaTime;

        IsSprinting = keys != null && keys.leftShiftKey.isPressed && movement.sqrMagnitude > 0.01f && !IsSlowed;
        float speedScale = IsSlowed ? slowMoveMultiplier : 1f;
        if (IsSprinting) speedScale *= sprintMultiplier;
        Vector3 targetVelocity = movement * moveSpeed * speedScale;

        bool onSlipperyGround = Time.frameCount - slipperyFrame <= 2;
        if (onSlipperyGround)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, slipperyGrip * Time.deltaTime);
        else
            horizontalVelocity = targetVelocity;

        // Turn the diver to face the way they are moving
        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion facing = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
        }

        movement = horizontalVelocity;
        movement.y = verticalSpeed;

        if (!IsGrounded) fallSpeed = Mathf.Min(fallSpeed, verticalSpeed);

        CollisionFlags flags = controller.Move(movement * Time.deltaTime);

        IsGrounded = (flags & CollisionFlags.Below) != 0;
        if (IsGrounded) lastGroundedTime = Time.time;

        // Small puff of sand when landing from a big fall
        if (IsGrounded && !wasGrounded && fallSpeed < -9f)
            DashKillable.Pop(transform.position + Vector3.down * 0.9f, new Color(0.85f, 0.8f, 0.65f));
        if (IsGrounded) fallSpeed = 0f;
        wasGrounded = IsGrounded;
    }

    // Used by jump pads (launch upwards) and respawning / air dash (pass 0 to stop falling)
    public void Launch(float upwardSpeed)
    {
        risingFromOwnJump = false;
        verticalSpeed = upwardSpeed;
    }

    // Called by PlayerPlatformContact every frame the player stands on a slippery platform
    public void SetOnSlipperyGround(float grip)
    {
        slipperyFrame = Time.frameCount;
        slipperyGrip = grip;
    }

    // Used by sticky trash: multiply speed and jump height for a few seconds
    public void ApplySlow(float moveMultiplier, float jumpMultiplier, float duration)
    {
        slowMoveMultiplier = moveMultiplier;
        slowJumpMultiplier = jumpMultiplier;
        slowUntil = Mathf.Max(slowUntil, Time.time + duration);
    }
}
