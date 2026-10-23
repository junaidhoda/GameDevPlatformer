
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
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastGroundedTime = -999f; // prevent a double jump within the grace window
        }

        verticalSpeed += gravity * Time.deltaTime;

        movement = movement * moveSpeed;
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
}
