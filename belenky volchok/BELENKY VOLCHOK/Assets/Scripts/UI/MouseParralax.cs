using UnityEngine;

public class MouseParallax : MonoBehaviour
{
    [SerializeField] private GameObject[] parallaxObjects;
    [SerializeField] private float mouseSpeedX = 0.5f;
    [SerializeField] private float mouseSpeedY = 0.3f;
    [SerializeField] private float smoothTime = 0.1f;
    
    private Vector3[] originalPositions;
    private Vector3[] velocities;
    private Vector3 currentOffset;
    private Vector3 targetOffset;
    
    void Start()
    {
        originalPositions = new Vector3[parallaxObjects.Length];
        velocities = new Vector3[parallaxObjects.Length];
        
        for (int i = 0; i < parallaxObjects.Length; i++)
        {
            originalPositions[i] = parallaxObjects[i].transform.position;
        }
    }
    
    void Update()
    {
        float x = (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
        float y = (Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f);
        
        targetOffset = new Vector3(x * mouseSpeedX, y * mouseSpeedY, 0);
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, smoothTime * 10f * Time.deltaTime);
        
        for (int i = 0; i < parallaxObjects.Length; i++)
        {
            float depthFactor = (float)(i + 1) / parallaxObjects.Length;
            Vector3 offset = currentOffset * depthFactor;
            
            parallaxObjects[i].transform.position = Vector3.SmoothDamp(
                parallaxObjects[i].transform.position,
                originalPositions[i] + offset,
                ref velocities[i],
                smoothTime
            );
        }
    }
}
