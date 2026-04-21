using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TightSpriteOutline : MonoBehaviour
{
    [SerializeField] private bool updateEveryFrame = true;
    [SerializeField] private bool hideOriginalSprite = false;
    
    private Outline outline;
    private SpriteRenderer spriteRenderer;
    private GameObject meshObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh generatedMesh;
    private Sprite lastSprite;
    
    void Start()
    {
        Setup();
    }
    
    void LateUpdate()
    {
        if (updateEveryFrame && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            if (spriteRenderer.sprite != lastSprite)
            {
                UpdateMesh();
                lastSprite = spriteRenderer.sprite;
            }
        }
    }
    
    void Setup()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        meshObject = new GameObject("OutlineMesh");
        meshObject.transform.SetParent(transform);
        meshObject.transform.localPosition = Vector3.zero;
        meshObject.transform.localRotation = Quaternion.identity;
        meshObject.transform.localScale = Vector3.one;
        
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        
        generatedMesh = new Mesh();
        generatedMesh.name = "TightOutlineMesh";
        meshFilter.mesh = generatedMesh;
        
        outline = meshObject.AddComponent<Outline>();
        
        UpdateMesh();
        lastSprite = spriteRenderer.sprite;
        
        meshRenderer.material = spriteRenderer.material;
        
        if (hideOriginalSprite)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    void UpdateMesh()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        
        Sprite sprite = spriteRenderer.sprite;
        Vector2[] verts2D = sprite.vertices;
        ushort[] tris2D = sprite.triangles;
        
        Vector3[] verts3D = new Vector3[verts2D.Length];
        for (int i = 0; i < verts2D.Length; i++)
        {
            verts3D[i] = new Vector3(
                verts2D[i].x / sprite.pixelsPerUnit,
                verts2D[i].y / sprite.pixelsPerUnit,
                0
            );
        }
        
        int[] triangles = new int[tris2D.Length];
        for (int i = 0; i < tris2D.Length; i++)
        {
            triangles[i] = tris2D[i];
        }
        
        generatedMesh.Clear();
        generatedMesh.vertices = verts3D;
        generatedMesh.triangles = triangles;
        generatedMesh.uv = sprite.uv;
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
    }
    
    public Outline GetOutline()
    {
        return outline;
    }
    
    void OnDestroy()
    {
        if (generatedMesh != null) Destroy(generatedMesh);
        if (meshObject != null) Destroy(meshObject);
    }
}
