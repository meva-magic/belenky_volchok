using UnityEngine;

public class MouseParalaxEffect : MonoBehaviour
{
    [Tooltip("Start from furthest to the nearest object.")]
    [SerializeField] private GameObject[] ParalaxObjects;
    [SerializeField] private float moveDistance = 20f;
    
    private Vector3[] OriginalPositions;
    private Vector2 currentMousePos;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        
        OriginalPositions = new Vector3[ParalaxObjects.Length];
        
        for (int i = 0; i < ParalaxObjects.Length; i++)
        {
            OriginalPositions[i] = ParalaxObjects[i].transform.position;
        }
    }

    void Update()
    {
        // Get mouse position (-1 to 1 range)
        float x = (Input.mousePosition.x - Screen.width / 2f) / (Screen.width / 2f);
        float y = (Input.mousePosition.y - Screen.height / 2f) / (Screen.height / 2f);
        
        // Smooth mouse movement
        currentMousePos = Vector2.Lerp(currentMousePos, new Vector2(x, y), Time.deltaTime * 8f);
        
        // Apply parallax to each object
        for (int i = 0; i < ParalaxObjects.Length; i++)
        {
            // Front objects (higher index) move more
            float depth = (float)i / (ParalaxObjects.Length - 1);
            float moveAmount = moveDistance * (depth * 1.5f + 0.3f);
            
            Vector3 offset = new Vector3(currentMousePos.x * moveAmount, currentMousePos.y * moveAmount * 0.5f, 0f);
            
            // Smooth movement
            ParalaxObjects[i].transform.position = Vector3.Lerp(
                ParalaxObjects[i].transform.position,
                OriginalPositions[i] + offset,
                Time.deltaTime * 5f
            );
        }
    }
}
