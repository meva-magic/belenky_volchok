using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.green;
    public float outlineWidth = 2f;
    
    [Header("Visibility Settings")]
    [Tooltip("If true, outline will be visible through other objects. If false, outline will be hidden when obscured.")]
    public bool showOutlineThroughObjects = false;
    
    public LayerMask targetLayer;

    [SerializeField] private GameObject selectedObj;
    private GameObject hoveredObj;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleHover();
        HandleSelection();
    }

    void HandleHover()
    {
        // Get mouse X position and create a vertical plane ray
        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // Find all objects in front of the camera
        Collider[] allColliders = Physics.OverlapSphere(mainCamera.transform.position, 100f, targetLayer);
        
        GameObject closestObject = null;
        float smallestAngle = float.MaxValue;
        float closestDistance = float.MaxValue;
        
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        // Project mouse ray onto horizontal plane to get direction
        Vector3 mouseDirection = mouseRay.direction;
        mouseDirection.y = 0;
        mouseDirection.Normalize();
        
        foreach (Collider col in allColliders)
        {
            Vector3 directionToTarget = col.transform.position - mainCamera.transform.position;
            float distance = directionToTarget.magnitude;
            directionToTarget.y = 0;
            directionToTarget.Normalize();
            
            // Check if object is roughly in the same horizontal direction as mouse
            float angle = Vector3.Angle(mouseDirection, directionToTarget);
            
            if (angle < 15f) // Within 15 degrees horizontally
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = col.gameObject;
                }
            }
        }
        
        if (closestObject != null)
        {
            if (hoveredObj != closestObject)
            {
                if (hoveredObj != null && hoveredObj != selectedObj)
                {
                    DisableOutline(hoveredObj);
                }
                
                hoveredObj = closestObject;
                if (hoveredObj != selectedObj)
                {
                    EnableOutline(hoveredObj, hoverColor);
                }
            }
        }
        else
        {
            if (hoveredObj != null && hoveredObj != selectedObj)
            {
                DisableOutline(hoveredObj);
                hoveredObj = null;
            }
        }
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hoveredObj != null)
            {
                if (selectedObj != null)
                {
                    DisableOutline(selectedObj);
                }
                
                selectedObj = hoveredObj;
                EnableOutline(selectedObj, selectedColor);
            }
            else
            {
                if (selectedObj != null)
                {
                    DisableOutline(selectedObj);
                    selectedObj = null;
                }
            }
        }
    }

    void EnableOutline(GameObject obj, Color color)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }
        
        outline.OutlineColor = color;
        outline.OutlineWidth = outlineWidth;
        
        if (showOutlineThroughObjects)
        {
            outline.OutlineMode = Outline.Mode.OutlineAll;
        }
        else
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }
        
        outline.EnableOutline(true);
    }

    void DisableOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.EnableOutline(false);
        }
    }
}