using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -10f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float slopeCheckDistance = 0.5f;
    
    private Vector3 inputVector;
    private Vector3 movementVector;
    private float verticalVelocity;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update() 
    {
        CheckGrounded();
        GetInput();
        HandleSlopeMovement();
        ApplyGravity();
        MovePlayer();
    }

    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
    }

    private void GetInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        inputVector = new Vector3(horizontal, 0, vertical);
        inputVector.Normalize();
        
        inputVector = transform.TransformDirection(inputVector);
        movementVector = inputVector * speed;
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void HandleSlopeMovement()
    {
        // Only handle slopes when grounded and moving
        if (!isGrounded || inputVector.magnitude < 0.1f)
            return;
        
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = inputVector.normalized;
        float rayLength = controller.radius + slopeCheckDistance;
        
        // Cast ray forward to detect slope
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            
            // If we hit a walkable slope
            if (slopeAngle > 0 && slopeAngle <= maxSlopeAngle)
            {
                // Calculate the direction along the slope
                Vector3 slopeDirection = Vector3.ProjectOnPlane(rayDirection, hit.normal).normalized;
                movementVector = slopeDirection * speed;
                
                // Add upward force to help climb the slope
                verticalVelocity = 0;
                movementVector.y = speed * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
            }
        }
        
        // Cast ray downward to check if we're already on a slope
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.3f))
        {
            float groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            
            // If we're on a slope, adjust gravity to keep us grounded
            if (groundAngle > 0 && groundAngle <= maxSlopeAngle && verticalVelocity <= 0)
            {
                // Project movement onto the slope plane
                movementVector = Vector3.ProjectOnPlane(movementVector, hit.normal);
                
                // Prevent bouncing down slopes
                verticalVelocity = -2f;
            }
        }
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
        movementVector.y = verticalVelocity;
    }

    private void MovePlayer()
    {
        controller.Move(movementVector * Time.deltaTime);
    }
    
    // Visualize slope detection in editor
    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;
        
        Gizmos.color = Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Vector3 forward = transform.forward * (controller.radius + slopeCheckDistance);
        Gizmos.DrawRay(rayOrigin, forward);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.3f);
    }
}