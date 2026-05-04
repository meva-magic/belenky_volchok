Shader "Custom/DoubleSidedLit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { 
            "RenderType"="TransparentCutout" 
            "Queue"="AlphaTest" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // URP lighting
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;
            half _Glossiness;
            half _Metallic;
            half _Cutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Standard vertex transformation (no billboarding)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                // Transform normal to world space
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample the sprite texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = texColor * _Color;
                
                // Discard transparent pixels
                clip(col.a - _Cutoff);
                
                // Get main directional light
                Light mainLight = GetMainLight();
                
                // Diffuse lighting (double-sided: use abs to light both sides)
                float NdotL = dot(input.normalWS, mainLight.direction);
                // Make lighting double-sided
                NdotL = abs(NdotL);
                half3 diffuse = mainLight.color * NdotL;
                
                // Specular lighting (Blinn-Phong)
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float specular = pow(max(0.0, abs(dot(input.normalWS, halfDir))), _Glossiness * 256);
                half3 specColor = mainLight.color * specular * _Metallic;
                
                // Ambient lighting
                half3 ambient = SampleSH(input.normalWS);
                
                // Shadow
                half shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(input.positionWS));
                
                // Combine main lighting
                half3 lighting = ambient + (diffuse * shadowAtten) + (specColor * shadowAtten);
                
                // Additional lights (spot lights, point lights)
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                
                for (uint lightIndex = 0; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light additionalLight = GetAdditionalLight(lightIndex, input.positionWS);
                    half3 attenuatedLightColor = additionalLight.color * additionalLight.distanceAttenuation * additionalLight.shadowAttenuation;
                    
                    // Diffuse for additional light (double-sided)
                    float addNdotL = abs(dot(input.normalWS, additionalLight.direction));
                    lighting += attenuatedLightColor * addNdotL;
                    
                    // Specular for additional light
                    float3 addHalfDir = normalize(additionalLight.direction + viewDir);
                    float addSpecular = pow(max(0.0, abs(dot(input.normalWS, addHalfDir))), _Glossiness * 256);
                    lighting += attenuatedLightColor * addSpecular * _Metallic;
                }
                #endif
                
                // Apply lighting to color
                col.rgb *= lighting;
                
                return col;
            }
            ENDHLSL
        }
        
        // Shadow casting pass with alpha clipping
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half _Cutoff;

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                
                // Standard vertex transformation (no billboarding)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                // Sample the texture and clip transparent pixels
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
