using UnityEngine;
public class FPSController : MonoBehaviour
{
    // Speed for player movement
    [SerializeField] float moveSpeed = 5.0f;
    // Speed for camera rotation
    [SerializeField] float lookSpeed = 3.0f;
    // Gravity applied to the player
    [SerializeField] float gravity = -9.81f;
    // Speed for crouching transition
    [SerializeField] float crouchSpeed = 5.0f;
    // Height of the character when crouching
    [SerializeField] float crouchHeight = 0.5f;

    // Reference to the CharacterController
    CharacterController m_characterController;
    // Reference to the main camera
    Camera m_camera;
    // Height of the character when standing
    float m_standingHeight;
    // Current height of the character
    float m_currentHeight;
    // Rotation angle for vertical camera movement
    float xRotation = 0f;
    // Velocity vector for gravity
    Vector3 m_velocity;
    // Initial position of the camera
    Vector3 m_cameraInitialPos;
    // Tracks if the player has landed
    bool m_hasLanded = false;
    // Tracks if the player can move
    bool m_canMove = false;

    // Determines if the player is crouching
    bool isCrouching => m_standingHeight - m_currentHeight > 0.1f;

    void Awake()
    {
        // Get the CharacterController and main camera references
        m_characterController = GetComponent<CharacterController>();
        m_camera = Camera.main;
    }

    void Start()
    {
        // Lock and hide the mouse cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize standing height and current height
        m_standingHeight = m_characterController.height;
        m_currentHeight = m_standingHeight;

        // Save the camera's initial position
        m_cameraInitialPos = m_camera.transform.localPosition;

        // Initialize the vertical velocity to a small downward value
        m_velocity.y = -2f;
    }

    void Update()
    {
        if (m_canMove)
            return;

        HandleCrouch();
        HandleMovement();
        HandleRotation();
        HandleGravity();
    }

    void HandleRotation()
    {
        // Get mouse input
        Vector2 mouseInput = GetMouseInput() * lookSpeed;

        if (mouseInput.magnitude > 0)
        {
            // Adjust vertical rotation and clamp it to prevent over-rotation
            xRotation -= mouseInput.y;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Rotate the camera vertically (up and down)
            m_camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Rotate the character horizontally (left and right)
            transform.Rotate(Vector3.up * mouseInput.x);
        }
    }

    void HandleMovement()
    {
        // Get move input
        Vector2 moveInput = GetMoveInput().normalized;

        // Check if there is any movement input
        if (moveInput.magnitude > 0)
        {
            // Calculate the movement direction based on input
            Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

            // Move the character
            m_characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    void HandleCrouch()
    {
        // Check if the player is trying to crouch
        bool tryingToCrouch = Input.GetKey(KeyCode.LeftControl);

        // Determine the target height for the character: 
        float targetHeight = tryingToCrouch ? crouchHeight : m_standingHeight;

        // Check if the player is currently crouching and attempting to stand up
        if (isCrouching && !tryingToCrouch)
        {
            // Check if there is an obstacle above the player
            float distanceAbove = CeilingCheck();

            // If there is an obstacle, adjust the target height to prevent collision
            if (distanceAbove > 0)
                targetHeight = Mathf.Max(m_currentHeight + distanceAbove - 0.1f, crouchHeight);
        }

        // Only adjust if the current height is not equal to the target height
        if (!Mathf.Approximately(targetHeight, m_currentHeight))
        {
            // Update the character controller's height
            m_currentHeight = Mathf.Lerp(m_currentHeight, targetHeight, Time.deltaTime * crouchSpeed);
            m_characterController.height = m_currentHeight;

            // Calculate the camera offset and adjust its local position
            Vector3 cameraOffset = new Vector3(0, (m_standingHeight - m_currentHeight) / 2, 0);
            m_camera.transform.localPosition = m_cameraInitialPos - cameraOffset;
        }
    }

    void HandleGravity()
    {
        // Check and handle landing
        HandleLanding();

        // Only apply gravity if the character is not grounded
        if (!GroundCheck())
        {
            m_velocity.y += gravity * Time.deltaTime;
            m_hasLanded = false; // Reset landing flag when airborne
        }

        // Move the character downward based on velocity
        m_characterController.Move(m_velocity * Time.deltaTime);
    }

    private void HandleLanding()
    {
        // If grounded, falling, and not already landed, handle landing
        if (GroundCheck() && m_velocity.y < 0 && !m_hasLanded)
        {
            m_velocity.y = -2f; // Reset vertical velocity
            m_hasLanded = true; // Mark as landed
        }
    }

    #region Utility
    float CeilingCheck()
    {
        // Checks if there is an obstruction above the player.
        Vector3 castOrigin = transform.position + new Vector3(0, m_currentHeight / 2, 0);
        if (Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, 0.2f))
        {
            // Return the distance to the ceiling.
            return hit.point.y - castOrigin.y;
        }
        return 0f;
    }

    bool GroundCheck()
    {
        // Check if there is an ground beneath the player
        return Physics.Raycast(transform.position, Vector3.down, m_currentHeight + 0.1f);
    }

    // Method to disable the script behaviour
    public void RestrictMovement(bool stopMovement)
    {
        m_canMove = stopMovement;
    }
    #endregion

    #region Inputs
    Vector2 GetMouseInput()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    Vector2 GetMoveInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    #endregion
}

