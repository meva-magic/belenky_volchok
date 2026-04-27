using UnityEngine;

public enum ColorReductionMode
{
    PerChannelQuantization,
    CustomPalette,
    InspectorPalette
}

public enum DitherMethod
{
    Bayer2x2,
    Bayer4x4,
    Bayer8x8,
    BlueNoise
}

[System.Serializable]
public struct PaletteColor
{
    [ColorUsage(false, false)]
    public Color color;
    
    public PaletteColor(Color c)
    {
        color = c;
    }
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
    
    [Header("Custom Palette (Texture)")]
    public Texture2D colorPalette;
    public bool invertPalette = false;
    
    [Header("Inspector Palette (Colors)")]
    public PaletteColor[] paletteColors = new PaletteColor[]
    {
        new PaletteColor(Color.black),
        new PaletteColor(new Color(0.25f, 0.25f, 0.25f)),
        new PaletteColor(new Color(0.5f, 0.5f, 0.5f)),
        new PaletteColor(new Color(0.75f, 0.75f, 0.75f)),
        new PaletteColor(Color.white)
    };
    
    [Header("Dithering")]
    [Range(0f, 1f)]
    public float ditherSpread = 0.5f;
    
    public DitherMethod ditherMethod = DitherMethod.Bayer4x4;
    
    [Header("Blue Noise (Optional)")]
    public Texture2D blueNoiseTexture;
    
    private Material retroMaterial;
    private Texture2D generatedPaletteTexture;
    
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
        
        if (colorReductionMode == ColorReductionMode.InspectorPalette)
        {
            GeneratePaletteFromColors();
        }
    }
    
    void OnDisable()
    {
        if (retroMaterial != null)
        {
            DestroyImmediate(retroMaterial);
            retroMaterial = null;
        }
        
        CleanupGeneratedTexture();
    }
    
    void OnValidate()
    {
        if (colorReductionMode == ColorReductionMode.InspectorPalette)
        {
            GeneratePaletteFromColors();
        }
    }
    
    void CleanupGeneratedTexture()
    {
        if (generatedPaletteTexture != null)
        {
            DestroyImmediate(generatedPaletteTexture);
            generatedPaletteTexture = null;
        }
    }
    
    void GeneratePaletteFromColors()
    {
        if (paletteColors == null || paletteColors.Length < 2)
        {
            CleanupGeneratedTexture();
            return;
        }
        
        CleanupGeneratedTexture();
        
        int width = 256;
        generatedPaletteTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        generatedPaletteTexture.filterMode = FilterMode.Point;
        generatedPaletteTexture.wrapMode = TextureWrapMode.Clamp;
        
        Color[] colors = new Color[width];
        
        for (int i = 0; i < width; i++)
        {
            float t = i / (float)(width - 1);
            float paletteT = t * (paletteColors.Length - 1);
            int index0 = Mathf.FloorToInt(paletteT);
            int index1 = Mathf.CeilToInt(paletteT);
            float frac = paletteT - index0;
            
            index0 = Mathf.Clamp(index0, 0, paletteColors.Length - 1);
            index1 = Mathf.Clamp(index1, 0, paletteColors.Length - 1);
            
            colors[i] = Color.Lerp(paletteColors[index0].color, paletteColors[index1].color, frac);
        }
        
        generatedPaletteTexture.SetPixels(colors);
        generatedPaletteTexture.Apply();
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
        
        if (colorReductionMode == ColorReductionMode.CustomPalette && colorPalette != null)
        {
            usePalette = true;
            activePalette = colorPalette;
        }
        else if (colorReductionMode == ColorReductionMode.InspectorPalette && generatedPaletteTexture != null)
        {
            usePalette = true;
            activePalette = generatedPaletteTexture;
        }
        
        retroMaterial.SetInt("_UsePalette", usePalette ? 1 : 0);
        
        if (usePalette && activePalette != null)
        {
            retroMaterial.SetTexture("_ColorPalette", activePalette);
            retroMaterial.SetInt("_PaletteInvert", invertPalette ? 1 : 0);
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
