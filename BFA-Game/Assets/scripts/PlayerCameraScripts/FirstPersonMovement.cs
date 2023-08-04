using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed of the character
    public float sprintSpeedMultiplier = 1.5f; // Sprint speed multiplier
    public float crouchSpeedMultiplier = 0.5f; // Crouch speed multiplier
    public float mouseSensitivity = 2f; // Mouse sensitivity for looking around
    public float jumpForce = 5f; // Initial jump force for the character
    public float gravity = -9.81f; // Gravitational force

    public float bobbingAmount = 0.05f; // Amount of head bobbing
    public float bobbingSpeed = 10f; // Speed of head bobbing
    public float bobbingMaxHeight = 0.3f; // Maximum height of head bobbing
    public float bobbingIdleSpeed = 3f; // Speed of head bobbing when standing still
    public float bobbingCrouchSpeed = 5f; // Speed of head bobbing when crouching and standing still

    public float cameraHeight = 1.7f; // Height of the camera from the character's center
    public float crouchHeight = 0.8f; // Height of the camera when crouching
    public float standingHeight = 1.7f; // Height of the character when standing
    public float crouchTransitionSpeed = 10f; // Speed of crouch stand-up and crouch-down transition

    private CharacterController characterController;
    private Camera playerCamera;
    private float xRotation = 0f;
    public bool isGrounded;
    private Vector3 playerVelocity;
    private float headBobTimer = 0f;
    private float bobbingAmountY = 0f;
    private bool isMoving;
    private bool isSprinting;
    private bool isCrouching;

    private Vector3 standingCameraPos; // Position of the camera when standing
    private Vector3 crouchingCameraPos; // Position of the camera when crouching

    private void Start()
    {
        // Get references to the CharacterController and Camera components
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;

        // Set the initial camera height
        standingCameraPos = playerCamera.transform.localPosition;
        standingCameraPos.y = cameraHeight;
        playerCamera.transform.localPosition = standingCameraPos;

        // Calculate the crouching camera position
        crouchingCameraPos = standingCameraPos;
        crouchingCameraPos.y = crouchHeight;
    }

    private void Update()
    {
        // Call the movement, rotation, jump, sprint, crouch, and head bobbing functions every frame
        HandleMovement();
        HandleRotation();
        HandleJump();
        HandleSprint();
        HandleCrouch();
        HandleHeadBobbing();
    }

    private void HandleMovement()
    {
        // Get input from the player
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Check if the player is moving in any direction
        isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        // Calculate the movement direction relative to the player's orientation
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        // Apply speed multiplier based on the movement mode (sprinting or crouching)
        float speedMultiplier = 1f;
        if (isSprinting && !isCrouching)
        {
            speedMultiplier = sprintSpeedMultiplier;
        }
        else if (isCrouching)
        {
            speedMultiplier = crouchSpeedMultiplier;
        }

        // Apply movement speed
        characterController.SimpleMove(moveDirection * moveSpeed * speedMultiplier);

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

    private void HandleSprint()
    {
        // Check if the sprint button (left shift key) is pressed and the character is moving
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;
    }

    private void HandleCrouch()
    {
        // Check if the crouch button (left control key) is being held
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // Smoothly transition the camera position when crouching
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, crouchingCameraPos, crouchTransitionSpeed * Time.deltaTime);
            characterController.height = Mathf.Lerp(characterController.height, crouchHeight, crouchTransitionSpeed * Time.deltaTime);
            isCrouching = true;
        }
        else
        {  
            // Check if there is enough space above the player to stand up
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position + characterController.center;
            bool canStandUp = !Physics.Raycast(raycastOrigin, Vector3.up, out hit, standingHeight - crouchHeight, ~LayerMask.GetMask("Player"));
            if (canStandUp)
            {
                // Smoothly transition the camera position when standing up from crouch
                playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, standingCameraPos, crouchTransitionSpeed * Time.deltaTime);
                characterController.height = Mathf.Lerp(characterController.height, standingHeight, crouchTransitionSpeed * Time.deltaTime);
                isCrouching = false;
            }
        }
    }

    private void HandleHeadBobbing()
    {
        // Calculate head bob movement based on the character's movement speed
        float speedMultiplier = isMoving ? (isSprinting ? bobbingSpeed * sprintSpeedMultiplier : (isCrouching ? bobbingCrouchSpeed : bobbingSpeed)) : bobbingIdleSpeed;
        float waveslice = Mathf.Sin(headBobTimer);
        headBobTimer += speedMultiplier * Time.deltaTime;
        if (headBobTimer > Mathf.PI * 2)
        {
            headBobTimer -= Mathf.PI * 2;
        }

        // Calculate head bob position
        if (waveslice != 0)
        {
            bobbingAmountY = waveslice * bobbingAmount;
        }

        // Apply the head bob movement to the camera's position
        Vector3 localCameraPos = playerCamera.transform.localPosition;
        float totalBobbing = bobbingAmountY + bobbingMaxHeight;
        localCameraPos.y = Mathf.Lerp(localCameraPos.y, totalBobbing, Time.deltaTime * 10f);
        playerCamera.transform.localPosition = localCameraPos;
    }
}