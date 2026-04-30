using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class PosterizeDitherCamera : MonoBehaviour
{
    [Header("Color Palette Settings")]
    public bool useCustomPalette = true;
    
    [ColorUsage(false, true)]
    public Color[] paletteColors = new Color[]
    {
        new Color(0.0f, 0.0f, 0.1f), // Deep dark blue
        new Color(0.2f, 0.1f, 0.3f), // Dark purple
        new Color(0.4f, 0.2f, 0.2f), // Dark red
        new Color(0.5f, 0.4f, 0.2f), // Brown
        new Color(0.6f, 0.5f, 0.4f), // Warm gray
        new Color(0.8f, 0.7f, 0.6f), // Light warm
        new Color(0.9f, 0.85f, 0.8f), // Almost white
        new Color(1.0f, 0.95f, 0.9f)  // White
    };
    
    [Range(2, 16)]
    public int quantizationSteps = 8;
    
    [Header("Dithering Settings")]
    [Range(0.0f, 1.0f)]
    public float spread = 0.5f;
    
    [Range(0, 2)]
    public int bayerLevel = 1;
    
    [Header("Effect Control")]
    public bool enableEffect = true;
    public bool inverseColors = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Material effectMaterial;
    private Camera targetCamera;
    
    private void OnEnable()
    {
        targetCamera = GetComponent<Camera>();
        InitializeMaterial();
        
        // Register for rendering callbacks
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        
        if (showDebugInfo)
            Debug.Log("PosterizeDither: Effect enabled and callbacks registered");
    }
    
    private void OnDisable()
    {
        // Unregister from rendering callbacks
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        
        Cleanup();
    }
    
    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera != targetCamera || !enableEffect)
            return;
        
        if (effectMaterial == null)
            InitializeMaterial();
            
        if (effectMaterial == null)
            return;
        
        // Create a command buffer for this camera
        CommandBuffer cmd = CommandBufferPool.Get("PosterizeDitherEffect");
        
        // Setup material properties
        UpdateMaterialProperties();
        
        // Get the camera's current target texture
        RenderTargetIdentifier cameraTarget = new RenderTargetIdentifier(
            BuiltinRenderTextureType.CameraTarget
        );
        
        // Create a temporary render texture
        int tempRT = Shader.PropertyToID("_TempPosterizeDither");
        cmd.GetTemporaryRT(tempRT, 
            camera.pixelWidth, 
            camera.pixelHeight, 
            0,
            FilterMode.Point,
            RenderTextureFormat.Default
        );
        
        // Apply effect: Camera Target -> Temp RT (with effect) -> Camera Target
        cmd.Blit(cameraTarget, tempRT, effectMaterial, 0);
        cmd.Blit(tempRT, cameraTarget);
        
        // Cleanup
        cmd.ReleaseTemporaryRT(tempRT);
        
        // Execute the command buffer
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
        
        if (showDebugInfo)
            Debug.Log($"PosterizeDither: Applied effect to {camera.name} at {camera.pixelWidth}x{camera.pixelHeight}");
    }
    
    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Cleanup if needed
    }
    
    private void InitializeMaterial()
    {
        if (effectMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(effectMaterial);
            else
                DestroyImmediate(effectMaterial);
        }
        
        Shader shader = Shader.Find("Hidden/PosterizeDither");
        if (shader != null)
        {
            effectMaterial = new Material(shader);
            effectMaterial.hideFlags = HideFlags.HideAndDontSave;
            
            if (showDebugInfo)
                Debug.Log("PosterizeDither: Material created successfully");
        }
        else
        {
            Debug.LogError("PosterizeDither: Could not find shader 'Hidden/PosterizeDither'. " +
                          "Make sure the shader file exists and is named correctly!");
        }
    }
    
    private void UpdateMaterialProperties()
    {
        if (effectMaterial == null) return;
        
        // Set palette (support up to 8 colors)
        int colorCount = Mathf.Min(paletteColors.Length, 8);
        Vector4[] paletteData = new Vector4[8];
        
        for (int i = 0; i < 8; i++)
        {
            if (i < colorCount && paletteColors[i] != null)
            {
                Color c = paletteColors[i];
                paletteData[i] = new Vector4(c.r, c.g, c.b, c.a);
            }
            else
            {
                paletteData[i] = Vector4.zero;
            }
        }
        
        effectMaterial.SetVectorArray("_PaletteColors", paletteData);
        effectMaterial.SetInt("_PaletteColorCount", Mathf.Max(1, colorCount));
        effectMaterial.SetInt("_UsePalette", useCustomPalette ? 1 : 0);
        effectMaterial.SetInt("_StepsPerChannel", Mathf.Max(2, quantizationSteps));
        effectMaterial.SetFloat("_Spread", spread);
        effectMaterial.SetInt("_BayerLevel", Mathf.Clamp(bayerLevel, 0, 2));
        effectMaterial.SetInt("_Invert", inverseColors ? 1 : 0);
    }
    
    private void Cleanup()
    {
        if (effectMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(effectMaterial);
            else
                DestroyImmediate(effectMaterial);
            effectMaterial = null;
        }
    }
    
    private void OnValidate()
    {
        // Apply changes in editor immediately
        if (enabled && gameObject.activeInHierarchy)
        {
            UpdateMaterialProperties();
        }
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
}
