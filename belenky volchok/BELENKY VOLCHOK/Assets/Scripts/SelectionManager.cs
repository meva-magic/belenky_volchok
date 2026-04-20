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

    void Update()
    {
        // Handle object hover
        HandleHover();
        
        // Handle object selection
        HandleSelection();
    }

    void HandleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Only raycast against the target layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            GameObject hitObject = hit.transform.gameObject;
            
            // If hovering over a different object
            if (hoveredObj != hitObject)
            {
                // Reset previous hovered object
                if (hoveredObj != null && hoveredObj != selectedObj)
                {
                    DisableOutline(hoveredObj);
                    Debug.Log("Hover ended on: " + hoveredObj.name);
                }
                
                // Highlight new hovered object (if not selected)
                hoveredObj = hitObject;
                if (hoveredObj != selectedObj)
                {
                    EnableOutline(hoveredObj, hoverColor);
                    Debug.Log("Hovering over: " + hoveredObj.name);
                }
            }
        }
        else
        {
            // No object being hovered
            if (hoveredObj != null && hoveredObj != selectedObj)
            {
                DisableOutline(hoveredObj);
                Debug.Log("Hover ended (no object)");
                hoveredObj = null;
            }
        }
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Only raycast against the target layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
            {
                GameObject clickedObject = hit.transform.gameObject;
                
                // Reset previous selection
                if (selectedObj != null)
                {
                    DisableOutline(selectedObj);
                    Debug.Log("Deselected: " + selectedObj.name);
                }
                
                // Select new object
                selectedObj = clickedObject;
                
                // Apply selection outline
                EnableOutline(selectedObj, selectedColor);
                Debug.Log("Selected: " + selectedObj.name);
                
                // Update hovered object if it's the same as selected
                if (hoveredObj == selectedObj)
                {
                    Debug.Log(selectedObj.name + " is now selected (was hovering)");
                }
            }
            else
            {
                // Clicked on nothing - clear selection
                if (selectedObj != null)
                {
                    DisableOutline(selectedObj);
                    Debug.Log("Deselected: " + selectedObj.name + " (clicked empty space)");
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
        
        // Choose mode based on visibility setting
        if (showOutlineThroughObjects)
        {
            // Outline will be visible through other objects
            outline.OutlineMode = Outline.Mode.OutlineAll;
        }
        else
        {
            // Outline will be hidden when object is obscured
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