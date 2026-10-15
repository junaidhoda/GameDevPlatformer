
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -15f;

    private CharacterController controller;
    private float verticalSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        Vector3 movement = new Vector3(input.x, 0, input.y).normalized;

        if (controller.isGrounded && verticalSpeed < 0)
            verticalSpeed = -2f;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            controller.isGrounded)
        {
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalSpeed += gravity * Time.deltaTime;

        movement = movement * moveSpeed;
        movement.y = verticalSpeed;

        controller.Move(movement * Time.deltaTime);
    }
}
