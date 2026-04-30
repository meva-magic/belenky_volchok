using UnityEngine;

public class PaletteSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class PalettePreset
    {
        public string presetName;
        public Texture2D paletteTexture;
        public int colorCount = 16;
        public int pixelSize = 4;
        public float ditherSpread = 0.5f;
        public bool smoothPalette = true;
    }
    
    public RetroShader retroShader;
    public PalettePreset[] palettes;
    
    void Update()
    {
        for (int i = 0; i < palettes.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ApplyPalette(i);
            }
        }
    }
    
    void ApplyPalette(int index)
    {
        if (retroShader == null || index >= palettes.Length) return;
        
        PalettePreset preset = palettes[index];
        
        retroShader.pixelSize = preset.pixelSize;
        retroShader.ditherSpread = preset.ditherSpread;
        retroShader.smoothPalette = preset.smoothPalette;
        retroShader.SetPaletteFromTexture(preset.paletteTexture, preset.colorCount);
    }
}
