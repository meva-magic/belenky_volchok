using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerMove playerMove;
    
    public enum PlayerState
    {
        Gameplay,
        UI
    }
    
    private PlayerState currentState;
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (mouseLook == null)
            mouseLook = GetComponentInChildren<MouseLook>();
        if (playerMove == null)
            playerMove = GetComponent<PlayerMove>();
        
        // Start in gameplay state
        SetGameplayState();
    }
    
    public void SetGameplayState()
    {
        currentState = PlayerState.Gameplay;
        
        // Enable player controls
        if (mouseLook != null)
            mouseLook.enabled = true;
        if (playerMove != null)
            playerMove.enabled = true;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void SetUIState()
    {
        currentState = PlayerState.UI;
        
        // Disable player controls
        if (mouseLook != null)
            mouseLook.enabled = false;
        if (playerMove != null)
            playerMove.enabled = false;
        
        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public PlayerState GetCurrentState()
    {
        return currentState;
    }
}