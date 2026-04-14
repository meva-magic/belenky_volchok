Shader "URP/DoubleSidedLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            Cull Off  // Double-sided shadows
            
            HLSLPROGRAM
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
            };

            Varyings vert_shadow(Attributes v)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }
            
            half4 frag_shadow(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // Main URP lit pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Off  // Double-sided rendering
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.fogCoord = ComputeFogFactor(o.vertex.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                clip(albedo.a - 0.01);
                
                // Get main light with shadow coordinates
                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                
                // Lighting calculation
                float3 normal = normalize(i.worldNormal);
                float NdotL = max(0, dot(normal, mainLight.direction));
                
                // Diffuse lighting
                float3 diffuse = albedo.rgb * mainLight.color * NdotL;
                
                // Ambient
                float3 ambient = albedo.rgb * SampleSH(normal);
                
                float3 finalColor = ambient + diffuse;
                finalColor = MixFog(finalColor, i.fogCoord);
                
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }
    }
}
