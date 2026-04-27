Shader "Hidden/RetroShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            // Pixelation
            int _PixelSize;
            
            // Color Palette
            sampler2D _ColorPalette;
            float4 _ColorPalette_TexelSize;
            int _UsePalette;
            int _PaletteInvert;
            
            // Color Quantization
            int _RedLevels;
            int _GreenLevels;
            int _BlueLevels;
            int _UseQuantization;
            
            // Dithering
            float _DitherSpread;
            int _DitherType;
            
            // Blue Noise Texture
            sampler2D _BlueNoiseTex;
            float4 _BlueNoiseTex_TexelSize;
            float _UseBlueNoise;

            // Pre-computed Bayer matrices
            static const float bayer2x2[4] = { 0.0, 2.0, 3.0, 1.0 };
            static const float bayer4x4[16] = 
            {
                0.0,  8.0,  2.0,  10.0,
                12.0, 4.0,  14.0, 6.0,
                3.0,  11.0, 1.0,  9.0,
                15.0, 7.0,  13.0, 5.0
            };
            static const float bayer8x8[64] = 
            {
                0.0,  32.0, 8.0,  40.0, 2.0,  34.0, 10.0, 42.0,
                48.0, 16.0, 56.0, 24.0, 50.0, 18.0, 58.0, 26.0,
                12.0, 44.0, 4.0,  36.0, 14.0, 46.0, 6.0,  38.0,
                60.0, 28.0, 52.0, 20.0, 62.0, 30.0, 54.0, 22.0,
                3.0,  35.0, 11.0, 43.0, 1.0,  33.0, 9.0,  41.0,
                51.0, 19.0, 59.0, 27.0, 49.0, 17.0, 57.0, 25.0,
                15.0, 47.0, 7.0,  39.0, 13.0, 45.0, 5.0,  37.0,
                63.0, 31.0, 55.0, 23.0, 61.0, 29.0, 53.0, 21.0
            };

            float GetBayer(int x, int y, int level)
            {
                if (level == 0)
                {
                    return bayer2x2[(x % 2) + (y % 2) * 2] / 4.0 - 0.5;
                }
                else if (level == 1)
                {
                    return bayer4x4[(x % 4) + (y % 4) * 4] / 16.0 - 0.5;
                }
                else
                {
                    return bayer8x8[(x % 8) + (y % 8) * 8] / 64.0 - 0.5;
                }
            }

            float GetDitherValue(float2 uv, float2 pixelPos)
            {
                int x = (int)pixelPos.x;
                int y = (int)pixelPos.y;
                
                if (_UseBlueNoise > 0.5)
                {
                    float2 noiseUV = float2(
                        x % _BlueNoiseTex_TexelSize.z,
                        y % _BlueNoiseTex_TexelSize.w
                    ) / _BlueNoiseTex_TexelSize.zw;
                    return tex2Dlod(_BlueNoiseTex, float4(noiseUV, 0, 0)).r - 0.5;
                }
                else
                {
                    return GetBayer(x, y, _DitherType);
                }
            }

            float Luminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            float3 ApplyPalette(float grayscale)
            {
                float paletteCoord = _PaletteInvert == 1 ? 1.0 - grayscale : grayscale;
                paletteCoord = saturate(paletteCoord);
                
                float paletteWidth = _ColorPalette_TexelSize.z;
                float exactPos = paletteCoord * paletteWidth;
                float index1 = floor(exactPos);
                float index2 = ceil(exactPos);
                float frac = exactPos - index1;
                
                float2 uv1 = float2(index1 / paletteWidth, 0.5);
                float2 uv2 = float2(index2 / paletteWidth, 0.5);
                
                float3 color1 = tex2Dlod(_ColorPalette, float4(uv1, 0, 0)).rgb;
                float3 color2 = tex2Dlod(_ColorPalette, float4(uv2, 0, 0)).rgb;
                
                return lerp(color1, color2, frac);
            }

            float3 QuantizeColorPerChannel(float3 color)
            {
                color.r = floor(color.r * (_RedLevels - 1) + 0.5) / max(1, _RedLevels - 1);
                color.g = floor(color.g * (_GreenLevels - 1) + 0.5) / max(1, _GreenLevels - 1);
                color.b = floor(color.b * (_BlueLevels - 1) + 0.5) / max(1, _BlueLevels - 1);
                return color;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Step 1: Pixelation
                float2 pixelUV;
                if (_PixelSize > 1)
                {
                    float2 pixelCoord = floor(i.uv * _MainTex_TexelSize.zw / _PixelSize) * _PixelSize;
                    pixelUV = pixelCoord / _MainTex_TexelSize.zw;
                }
                else
                {
                    pixelUV = i.uv;
                }
                
                float2 pixelPos = pixelUV * _MainTex_TexelSize.zw;
                
                // Step 2: Sample the color at the pixelated coordinate
                float3 color = tex2D(_MainTex, pixelUV).rgb;
                
                // Step 3: Apply dithering
                float dither = GetDitherValue(i.uv, pixelPos) * _DitherSpread;
                color += dither;
                color = saturate(color);
                
                // Step 4: Apply color reduction
                if (_UsePalette)
                {
                    float luminance = Luminance(color);
                    color = ApplyPalette(luminance);
                }
                else
                {
                    color = QuantizeColorPerChannel(color);
                }
                
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
