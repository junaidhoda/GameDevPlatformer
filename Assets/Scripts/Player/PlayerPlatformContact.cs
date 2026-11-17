using UnityEngine;

// Lets the player interact with special platforms they land on:
// breaking platforms start to crumble, jump pads launch, moving platforms carry,
// slippery platforms make you slide,
// and the level goal opens the Level Complete menu.
[RequireComponent(typeof(CharacterController))]
public class PlayerPlatformContact : MonoBehaviour
{
    private CharacterController controller;
    private PlayerMovement movement;
    private MovingPlatform currentMover;
    private int lastMoverFrame = -1;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Only react to things the player is standing on, not walls
        if (hit.normal.y < 0.5f) return;

        BreakingPlatform breaking = hit.collider.GetComponentInParent<BreakingPlatform>();
        if (breaking != null) breaking.StepOn();

        JumpPad pad = hit.collider.GetComponentInParent<JumpPad>();
        if (pad != null) pad.Launch(gameObject);

        SlipperyPlatform slippery = hit.collider.GetComponentInParent<SlipperyPlatform>();
        if (slippery != null && movement != null) movement.SetOnSlipperyGround(slippery.grip);

        LevelGoal goal = hit.collider.GetComponentInParent<LevelGoal>();
        if (goal != null) goal.Reach();

        MovingPlatform mover = hit.collider.GetComponentInParent<MovingPlatform>();
        if (mover != null)
        {
            currentMover = mover;
            lastMoverFrame = Time.frameCount;
        }
    }

    void LateUpdate()
    {
        if (currentMover == null) return;

        // Still standing on it this frame or last frame? Move with it.
        if (Time.frameCount - lastMoverFrame <= 1 && controller.enabled)
        {
            controller.Move(currentMover.Delta);
        }
        else
        {
            currentMover = null;
        }
    }
}
