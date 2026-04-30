using UnityEngine;

public enum ColorReductionMode
{
    PerChannelQuantization,
    CustomPalette,
    PaletteTexture
}

public enum DitherMethod
{
    Bayer2x2,
    Bayer4x4,
    Bayer8x8,
    BlueNoise
}

[RequireComponent(typeof(Camera))]
public class RetroShader : MonoBehaviour
{
    [Header("Shader")]
    public Shader retroShader;
    
    [Header("Pixelation")]
    [Range(1, 64)]
    public int pixelSize = 4;
    
    [Header("Color Reduction Mode")]
    public ColorReductionMode colorReductionMode = ColorReductionMode.PerChannelQuantization;
    
    [Header("Per-Channel Quantization")]
    [Range(2, 256)]
    public int redLevels = 8;
    [Range(2, 256)]
    public int greenLevels = 8;
    [Range(2, 256)]
    public int blueLevels = 8;
    
    [Header("Palette Texture")]
    [Tooltip("PNG with colors organized left to right. Width = number of colors, Height = 1 pixel")]
    public Texture2D paletteTexture;
    
    [Tooltip("Number of colors in the palette texture")]
    [Range(2, 256)]
    public int paletteColorCount = 16;
    
    [Tooltip("Smoothly interpolate between palette colors")]
    public bool smoothPalette = true;
    
    [Tooltip("Reverse the palette order")]
    public bool invertPalette = false;
    
    [Header("Dithering")]
    [Range(0f, 1f)]
    public float ditherSpread = 0.5f;
    
    public DitherMethod ditherMethod = DitherMethod.Bayer4x4;
    
    [Header("Blue Noise (Optional)")]
    public Texture2D blueNoiseTexture;
    
    private Material retroMaterial;
    private Texture2D processedPaletteTexture;
    
    void OnEnable()
    {
        if (retroShader == null)
        {
            Debug.LogError("Retro Shader not assigned!");
            return;
        }
        
        retroMaterial = new Material(retroShader);
        retroMaterial.hideFlags = HideFlags.HideAndDontSave;
        
        if (blueNoiseTexture == null && ditherMethod == DitherMethod.BlueNoise)
        {
            blueNoiseTexture = GenerateBlueNoise64x64();
        }
        
        if (colorReductionMode == ColorReductionMode.PaletteTexture && paletteTexture != null)
        {
            ProcessPaletteTexture();
        }
    }
    
    void OnDisable()
    {
        if (retroMaterial != null)
        {
            DestroyImmediate(retroMaterial);
            retroMaterial = null;
        }
        
        CleanupProcessedTexture();
    }
    
    void OnValidate()
    {
        if (colorReductionMode == ColorReductionMode.PaletteTexture && paletteTexture != null)
        {
            ProcessPaletteTexture();
        }
    }
    
    void CleanupProcessedTexture()
    {
        if (processedPaletteTexture != null)
        {
            DestroyImmediate(processedPaletteTexture);
            processedPaletteTexture = null;
        }
    }
    
    void ProcessPaletteTexture()
    {
        if (paletteTexture == null)
        {
            CleanupProcessedTexture();
            return;
        }
        
        CleanupProcessedTexture();
        
        // Read palette colors from the texture
        Color[] sourceColors = paletteTexture.GetPixels(0, 0, paletteTexture.width, 1);
        
        if (sourceColors.Length < 2)
        {
            Debug.LogWarning("Palette texture needs at least 2 colors");
            return;
        }
        
        // Create a 256x1 texture for smooth lookup
        int outputWidth = 256;
        processedPaletteTexture = new Texture2D(outputWidth, 1, TextureFormat.RGBA32, false);
        processedPaletteTexture.filterMode = smoothPalette ? FilterMode.Bilinear : FilterMode.Point;
        processedPaletteTexture.wrapMode = TextureWrapMode.Clamp;
        
        Color[] outputColors = new Color[outputWidth];
        
        // Use only the specified number of colors from the texture
        int colorCount = Mathf.Min(paletteColorCount, paletteTexture.width);
        int sampleWidth = paletteTexture.width / colorCount;
        
        // Sample center of each color region
        Color[] sampledColors = new Color[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            int x = (sampleWidth / 2) + (i * sampleWidth);
            x = Mathf.Clamp(x, 0, paletteTexture.width - 1);
            sampledColors[i] = sourceColors[x];
        }
        
        // Fill the 256-pixel lookup texture
        for (int i = 0; i < outputWidth; i++)
        {
            float t = i / (float)(outputWidth - 1);
            
            if (invertPalette)
                t = 1.0f - t;
            
            if (smoothPalette)
            {
                // Interpolate between nearest colors
                float paletteT = t * (colorCount - 1);
                int index0 = Mathf.FloorToInt(paletteT);
                int index1 = Mathf.CeilToInt(paletteT);
                float frac = paletteT - index0;
                
                index0 = Mathf.Clamp(index0, 0, colorCount - 1);
                index1 = Mathf.Clamp(index1, 0, colorCount - 1);
                
                outputColors[i] = Color.Lerp(sampledColors[index0], sampledColors[index1], frac);
            }
            else
            {
                // Snap to nearest color (hard quantization)
                int index = Mathf.FloorToInt(t * colorCount);
                index = Mathf.Clamp(index, 0, colorCount - 1);
                outputColors[i] = sampledColors[index];
            }
        }
        
        processedPaletteTexture.SetPixels(outputColors);
        processedPaletteTexture.Apply();
    }
    
    // Public methods for runtime switching
    public void SetPaletteFromTexture(Texture2D newPalette, int colorCount)
    {
        paletteTexture = newPalette;
        paletteColorCount = colorCount;
        colorReductionMode = ColorReductionMode.PaletteTexture;
        ProcessPaletteTexture();
    }
    
    public void SetGrayscale()
    {
        colorReductionMode = ColorReductionMode.PerChannelQuantization;
        redLevels = 256;
        greenLevels = 256;
        blueLevels = 256;
    }
    
    public void SetFullColor()
    {
        colorReductionMode = ColorReductionMode.PerChannelQuantization;
        redLevels = 256;
        greenLevels = 256;
        blueLevels = 256;
        pixelSize = 1;
        ditherSpread = 0;
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (retroMaterial == null || retroShader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        retroMaterial.SetInt("_PixelSize", pixelSize);
        
        bool usePalette = false;
        Texture2D activePalette = null;
        
        if (colorReductionMode == ColorReductionMode.CustomPalette && paletteTexture != null)
        {
            usePalette = true;
            activePalette = paletteTexture;
        }
        else if (colorReductionMode == ColorReductionMode.PaletteTexture && processedPaletteTexture != null)
        {
            usePalette = true;
            activePalette = processedPaletteTexture;
        }
        
        retroMaterial.SetInt("_UsePalette", usePalette ? 1 : 0);
        
        if (usePalette && activePalette != null)
        {
            retroMaterial.SetTexture("_ColorPalette", activePalette);
            retroMaterial.SetInt("_PaletteInvert", 0); // Already handled in processing
        }
        else
        {
            retroMaterial.SetInt("_RedLevels", redLevels);
            retroMaterial.SetInt("_GreenLevels", greenLevels);
            retroMaterial.SetInt("_BlueLevels", blueLevels);
        }
        
        retroMaterial.SetFloat("_DitherSpread", ditherSpread);
        retroMaterial.SetInt("_DitherType", (int)ditherMethod);
        
        if (ditherMethod == DitherMethod.BlueNoise && blueNoiseTexture != null)
        {
            retroMaterial.SetTexture("_BlueNoiseTex", blueNoiseTexture);
            retroMaterial.SetFloat("_UseBlueNoise", 1.0f);
        }
        else
        {
            retroMaterial.SetFloat("_UseBlueNoise", 0.0f);
        }
        
        Graphics.Blit(source, destination, retroMaterial);
    }
    
    Texture2D GenerateBlueNoise64x64()
    {
        Texture2D noise = new Texture2D(64, 64, TextureFormat.R8, false);
        noise.filterMode = FilterMode.Point;
        noise.wrapMode = TextureWrapMode.Repeat;
        
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(Random.value, 0, 0, 1);
        }
        
        for (int iterations = 0; iterations < 3; iterations++)
        {
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float value = pixels[y * 64 + x].r;
                    float sum = 0;
                    float count = 0;
                    
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            int sx = (x + dx + 64) % 64;
                            int sy = (y + dy + 64) % 64;
                            sum += pixels[sy * 64 + sx].r;
                            count++;
                        }
                    }
                    
                    float avg = sum / count;
                    pixels[y * 64 + x].r = Mathf.Clamp01(avg + (value - avg) * 1.5f);
                }
            }
        }
        
        noise.SetPixels(pixels);
        noise.Apply();
        return noise;
    }
}
