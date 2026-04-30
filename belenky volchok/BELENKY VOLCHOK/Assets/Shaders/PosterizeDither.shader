Shader "UI/PosterizeDither"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        [Header(Color Palette)]
        _Color0 ("Color 0", Color) = (0, 0, 0, 1)
        _Color1 ("Color 1", Color) = (0.25, 0.25, 0.25, 1)
        _Color2 ("Color 2", Color) = (0.5, 0.5, 0.5, 1)
        _Color3 ("Color 3", Color) = (0.75, 0.75, 0.75, 1)
        _Color4 ("Color 4", Color) = (1, 1, 1, 1)
        _Color5 ("Color 5", Color) = (0, 0, 0, 1)
        _Color6 ("Color 6", Color) = (0, 0, 0, 1)
        _Color7 ("Color 7", Color) = (0, 0, 0, 1)
        _PaletteCount ("Palette Count", Range(1, 8)) = 5
        
        [Header(Dithering)]
        _Spread ("Dither Spread", Range(0, 1)) = 0.5
        [Enum(2x2,0,4x4,1,8x8,2)] _BayerLevel ("Bayer Level", Float) = 1
        
        [Header(Options)]
        _Invert ("Invert", Range(0, 1)) = 0
        
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
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            fixed4 _Color0;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _Color4;
            fixed4 _Color5;
            fixed4 _Color6;
            fixed4 _Color7;
            int _PaletteCount;
            
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
                // Get pixel coordinates
                int x = (int)(uv.x * _MainTex_TexelSize.z);
                int y = (int)(uv.y * _MainTex_TexelSize.w);
                
                if (_BayerLevel == 0)
                    return bayer2[(x % 2) + (y % 2) * 2] / 4.0 - 0.5;
                else if (_BayerLevel == 1)
                    return bayer4[(x % 4) + (y % 4) * 4] / 16.0 - 0.5;
                else
                    return bayer8[(x % 8) + (y % 8) * 8] / 64.0 - 0.5;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample the texture
                fixed4 color = tex2D(_MainTex, IN.texcoord);
                color *= IN.color;

                // Invert if enabled
                if (_Invert == 1)
                    color.rgb = 1.0 - color.rgb;

                // Apply dithering
                float bayerValue = GetBayerValue(IN.texcoord);
                color.rgb += _Spread * bayerValue;
                color.rgb = saturate(color.rgb);

                // Create palette array
                fixed4 palette[8];
                palette[0] = _Color0;
                palette[1] = _Color1;
                palette[2] = _Color2;
                palette[3] = _Color3;
                palette[4] = _Color4;
                palette[5] = _Color5;
                palette[6] = _Color6;
                palette[7] = _Color7;

                // Find nearest color in palette
                fixed3 result = palette[0].rgb;
                float minDist = 1000.0; // Large initial value
                
                for (int i = 0; i < _PaletteCount; i++)
                {
                    float dist = distance(color.rgb, palette[i].rgb);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        result = palette[i].rgb;
                    }
                }

                color.rgb = result;

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
