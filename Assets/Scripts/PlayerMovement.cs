
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -15f;

    [Tooltip("Grace period (seconds) after leaving the ground where a jump still works")]
    public float coyoteTime = 0.1f;

    private CharacterController controller;
    private float verticalSpeed;
    private float lastGroundedTime = -999f;

    // Grounded state from THIS script's own Move call.
    // controller.isGrounded can't be trusted on its own, because other scripts
    // (e.g. PlayerPlatformContact carrying the player on a moving platform)
    // call controller.Move afterwards and overwrite it.
    public bool IsGrounded { get; private set; }

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

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        Vector3 movement = new Vector3(input.x, 0, input.y).normalized;

        if (IsGrounded && verticalSpeed < 0)
            verticalSpeed = -2f;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            canJump)
        {
            float jumpScale = IsSlowed ? slowJumpMultiplier : 1f;
            verticalSpeed = Mathf.Sqrt(jumpHeight * jumpScale * -2f * gravity);
            lastGroundedTime = -999f; // prevent a double jump within the grace window
        }

        verticalSpeed += gravity * Time.deltaTime;

        float speedScale = IsSlowed ? slowMoveMultiplier : 1f;
        Vector3 targetVelocity = movement * moveSpeed * speedScale;

        bool onSlipperyGround = Time.frameCount - slipperyFrame <= 2;
        if (onSlipperyGround)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, slipperyGrip * Time.deltaTime);
        else
            horizontalVelocity = targetVelocity;

        movement = horizontalVelocity;
        movement.y = verticalSpeed;

        CollisionFlags flags = controller.Move(movement * Time.deltaTime);

        IsGrounded = (flags & CollisionFlags.Below) != 0;
        if (IsGrounded) lastGroundedTime = Time.time;
    }

    // Used by jump pads (launch upwards) and respawning / air dash (pass 0 to stop falling)
    public void Launch(float upwardSpeed)
    {
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
