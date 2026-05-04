using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -10f;
    [SerializeField] private float jumpForce = 5f;
    
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
        ApplyGravity();
        MovePlayer();
    }

    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small downward force to keep grounded
        }
    }

    private void GetInput()
    {
        // Get movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        inputVector = new Vector3(horizontal, 0, vertical);
        inputVector.Normalize();
        
        // Transform input to world space relative to player orientation
        inputVector = transform.TransformDirection(inputVector);
        
        // Create movement vector (without gravity yet)
        movementVector = inputVector * speed;
        
        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        movementVector.y = verticalVelocity;
    }

    private void MovePlayer()
    {
        controller.Move(movementVector * Time.deltaTime);
        
        if (inputVector.magnitude > 0 && isGrounded)
        {
            AudioManager.instance.Play("Footsteps");
        }
        else
        {
            AudioManager.instance.Stop("Footsteps");
        }
    }
}
