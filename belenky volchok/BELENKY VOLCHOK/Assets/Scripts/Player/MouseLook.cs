using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    [SerializeField] private float sensitivity = 1.5f;
    [SerializeField] private float smoothing = 1.5f;
    
    [Header("Vertical Look Limits")]
    [SerializeField] private bool limitVerticalLook = true;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    
    private float currentHorizontalLook;
    private float currentVerticalLook;
    
    private float xMousePos;
    private float yMousePos;
    private float smoothedMouseX;
    private float smoothedMouseY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        GetInput();
        ModifyInput();
        MovePlayer();
    }

    private void GetInput()
    {
        xMousePos = Input.GetAxisRaw("Mouse X");
        yMousePos = Input.GetAxisRaw("Mouse Y");
    }

    private void ModifyInput()
    {
        xMousePos *= sensitivity;
        yMousePos *= sensitivity;
        
        smoothedMouseX = Mathf.Lerp(smoothedMouseX, xMousePos, 1f / smoothing);
        smoothedMouseY = Mathf.Lerp(smoothedMouseY, yMousePos, 1f / smoothing);
    }

    private void MovePlayer()
    {
        // Horizontal rotation (left/right) - exactly like your original
        currentHorizontalLook += smoothedMouseX;
        transform.localRotation = Quaternion.AngleAxis(currentHorizontalLook, Vector3.up);
        
        // Vertical rotation (up/down) - NEW
        currentVerticalLook -= smoothedMouseY;
        
        if (limitVerticalLook)
        {
            currentVerticalLook = Mathf.Clamp(currentVerticalLook, minVerticalAngle, maxVerticalAngle);
        }
        
        // Combine both rotations
        transform.localRotation = Quaternion.Euler(currentVerticalLook, currentHorizontalLook, 0f);
    }
}