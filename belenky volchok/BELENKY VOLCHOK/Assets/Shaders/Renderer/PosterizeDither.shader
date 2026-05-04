Shader "UI/PosterizeDither"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        [Header(Color Palette)]
        _ColorPalette ("Color Palette", 2D) = "white" {}
        
        [Header(Dithering)]
        _Spread ("Dither Spread", Range(0, 1)) = 0.5
        [Enum(2x2,0,4x4,1,8x8,2)] _BayerLevel ("Bayer Level", Float) = 1
        
        [Header(Options)]
        [Toggle] _Invert ("Invert", Float) = 0
        
        // Required for UI masking
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ColorMask [_ColorMask]

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "PosterizeDither"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _ColorPalette;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _ColorPalette_TexelSize;
            
            float _Spread;
            int _BayerLevel;
            int _Invert;

            // Bayer matrices
            static const float bayer2[4] = { 0, 2, 3, 1 };
            static const float bayer4[16] = {
                0,  8,  2, 10,
                12, 4, 14,  6,
                3, 11,  1,  9,
                15, 7, 13,  5
            };
            static const float bayer8[64] = {
                0, 32,  8, 40,  2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,
                12, 44,  4, 36, 14, 46,  6, 38,
                60, 28, 52, 20, 62, 30, 54, 22,
                3, 35, 11, 43,  1, 33,  9, 41,
                51, 19, 59, 27, 49, 17, 57, 25,
                15, 47,  7, 39, 13, 45,  5, 37,
                63, 31, 55, 23, 61, 29, 53, 21
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            float GetBayerValue(float2 uv)
            {
                int x = (int)(uv.x * _MainTex_TexelSize.z);
                int y = (int)(uv.y * _MainTex_TexelSize.w);
                
                if (_BayerLevel == 0)
                    return bayer2[(x % 2) + (y % 2) * 2] / 4.0 - 0.5;
                else if (_BayerLevel == 1)
                    return bayer4[(x % 4) + (y % 4) * 4] / 16.0 - 0.5;
                else
                    return bayer8[(x % 8) + (y % 8) * 8] / 64.0 - 0.5;
            }

            float ColorToGrayscale(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample the texture
                fixed4 color = tex2D(_MainTex, IN.texcoord);
                color *= IN.color;

                // Invert if enabled
                float paletteInput = ColorToGrayscale(color.rgb);
                if (_Invert == 1)
                    paletteInput = 1.0 - paletteInput;

                // Apply dithering to grayscale value for palette lookup
                float bayerValue = GetBayerValue(IN.texcoord);
                paletteInput += _Spread * bayerValue;
                paletteInput = saturate(paletteInput);

                // Sample palette using the method from PaletteSwapper.shader
                // Use floor/ceil for proper point sampling with interpolation
                float paletteWidth = _ColorPalette_TexelSize.z;
                float paletteIndex = paletteInput * paletteWidth;
                
                float firstIndex = floor(paletteIndex) / paletteWidth;
                float secondIndex = ceil(paletteIndex) / paletteWidth;
                
                float4 firstColor = tex2D(_ColorPalette, float2(firstIndex + (0.5 / paletteWidth), 0.5));
                float4 secondColor = tex2D(_ColorPalette, float2(secondIndex + (0.5 / paletteWidth), 0.5));
                
                float4 paletteColor = lerp(firstColor, secondColor, frac(paletteIndex));
                
                color.rgb = paletteColor.rgb;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}