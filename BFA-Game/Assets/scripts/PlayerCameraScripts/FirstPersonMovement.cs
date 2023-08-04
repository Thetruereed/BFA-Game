using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed of the character
    public float mouseSensitivity = 2f; // Mouse sensitivity for looking around
    public float jumpForce = 5f; // Initial jump force for the character
    public float gravity = -9.81f; // Gravitational force

    private CharacterController characterController;
    private Camera playerCamera;
    private float xRotation = 0f;
    private bool isGrounded;
    private Vector3 playerVelocity;

    private void Start()
    {
        // Get references to the CharacterController and Camera components
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Call the movement, rotation, and jump functions every frame
        HandleMovement();
        HandleRotation();
        HandleJump();
    }

    private void HandleMovement()
    {
        // Get input from the player
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate the movement direction relative to the player's orientation
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        // Apply movement speed
        characterController.SimpleMove(moveDirection * moveSpeed);

        // Check if the character is grounded using a raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);

        // Apply gravity to the character's vertical velocity
        if (isGrounded && playerVelocity.y < 0f)
        {
            playerVelocity.y = -2f; // A small negative value to ensure the character sticks to the ground
        }
        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Get mouse input to rotate the camera and the character
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the character left/right
        transform.Rotate(Vector3.up * mouseX);

        // Calculate the new vertical rotation for the camera
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp the vertical rotation to avoid over-rotation

        // Rotate the camera up/down
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleJump()
    {
        // Check if the jump button (space key) is pressed and the character is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Apply the initial jump force to the character's vertical velocity
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
}
