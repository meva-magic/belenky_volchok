using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public Material defaultMat;
    public Material highlightMat;
    public Material selectedMat;

    public LayerMask targetLayer; // Changed from LayoutMask to LayerMask

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
                    MeshRenderer prevRenderer = hoveredObj.GetComponent<MeshRenderer>();
                    if (prevRenderer != null)
                    {
                        prevRenderer.material = defaultMat;
                        Debug.Log("Hover ended on: " + hoveredObj.name);
                    }
                }
                
                // Highlight new hovered object (if not selected)
                hoveredObj = hitObject;
                if (hoveredObj != selectedObj)
                {
                    MeshRenderer newRenderer = hoveredObj.GetComponent<MeshRenderer>();
                    if (newRenderer != null)
                    {
                        newRenderer.material = highlightMat;
                        Debug.Log("Hovering over: " + hoveredObj.name);
                    }
                }
            }
        }
        else
        {
            // No object being hovered
            if (hoveredObj != null && hoveredObj != selectedObj)
            {
                MeshRenderer prevRenderer = hoveredObj.GetComponent<MeshRenderer>();
                if (prevRenderer != null)
                {
                    prevRenderer.material = defaultMat;
                    Debug.Log("Hover ended (no object)");
                }
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
                    MeshRenderer prevRenderer = selectedObj.GetComponent<MeshRenderer>();
                    if (prevRenderer != null)
                    {
                        prevRenderer.material = defaultMat;
                        Debug.Log("Deselected: " + selectedObj.name);
                    }
                }
                
                // Select new object
                selectedObj = clickedObject;
                
                // Apply selection material
                MeshRenderer newRenderer = selectedObj.GetComponent<MeshRenderer>();
                if (newRenderer != null)
                {
                    newRenderer.material = selectedMat;
                    Debug.Log("Selected: " + selectedObj.name);
                }
                
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
                    MeshRenderer prevRenderer = selectedObj.GetComponent<MeshRenderer>();
                    if (prevRenderer != null)
                    {
                        prevRenderer.material = defaultMat;
                        Debug.Log("Deselected: " + selectedObj.name + " (clicked empty space)");
                    }
                    selectedObj = null;
                }
            }
        }
    }
}
