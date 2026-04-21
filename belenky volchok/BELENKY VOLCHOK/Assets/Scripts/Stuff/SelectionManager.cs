using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.green;
    public float outlineWidth = 2f;
    
    public LayerMask targetLayer;
    
    private GameObject selectedObj;
    private GameObject hoveredObj;
    private Camera mainCamera;
    
    public string hoveredObjectName = "";
    public string selectedObjectName = "";
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        HandleHover();
        HandleClick();
    }
    
    void HandleHover()
    {
        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Collider[] allColliders = Physics.OverlapSphere(mainCamera.transform.position, 100f, targetLayer);
        
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;
        
        Vector3 mouseDir = mouseRay.direction;
        mouseDir.y = 0;
        mouseDir.Normalize();
        
        foreach (Collider col in allColliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null) continue;
            
            Vector3 dirToTarget = col.transform.position - mainCamera.transform.position;
            float distance = dirToTarget.magnitude;
            dirToTarget.y = 0;
            dirToTarget.Normalize();
            
            float angle = Vector3.Angle(mouseDir, dirToTarget);
            
            if (angle < 15f && distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = col.gameObject;
            }
        }
        
        if (closestObject != null)
        {
            if (hoveredObj != closestObject)
            {
                if (hoveredObj != null && hoveredObj != selectedObj)
                    DisableOutline(hoveredObj);
                
                hoveredObj = closestObject;
                hoveredObjectName = hoveredObj.name;
                
                if (hoveredObj != selectedObj)
                    EnableOutline(hoveredObj, hoverColor);
            }
        }
        else
        {
            if (hoveredObj != null && hoveredObj != selectedObj)
            {
                DisableOutline(hoveredObj);
                hoveredObj = null;
                hoveredObjectName = "";
            }
        }
    }
    
    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hoveredObj != null)
            {
                IInteractable interactable = hoveredObj.GetComponent<IInteractable>();
                
                if (interactable != null)
                {
                    if (selectedObj != null)
                        DisableOutline(selectedObj);
                    
                    selectedObj = hoveredObj;
                    selectedObjectName = selectedObj.name;
                    EnableOutline(selectedObj, selectedColor);
                    
                    interactable.Interact();
                }
            }
            else
            {
                if (selectedObj != null)
                {
                    DisableOutline(selectedObj);
                    selectedObj = null;
                    selectedObjectName = "";
                }
            }
        }
    }
    
    void EnableOutline(GameObject obj, Color color)
    {
        Outline outline = GetOutlineFromObject(obj);
        if (outline != null)
        {
            outline.OutlineColor = color;
            outline.OutlineWidth = outlineWidth;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.EnableOutline(true);
        }
    }
    
    void DisableOutline(GameObject obj)
    {
        Outline outline = GetOutlineFromObject(obj);
        if (outline != null)
            outline.EnableOutline(false);
    }
    
    Outline GetOutlineFromObject(GameObject obj)
    {
        TightSpriteOutline tight = obj.GetComponent<TightSpriteOutline>();
        if (tight != null)
            return tight.GetOutline();
        
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
            outline = obj.GetComponentInChildren<Outline>();
        
        return outline;
    }
}
