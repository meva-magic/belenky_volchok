Shader "Custom/StandardBillboardLit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Brightness ("Brightness", Range(0.5, 3)) = 1.0
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
                float4 shadowCoord : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;
            half _Glossiness;
            half _Metallic;
            half _Cutoff;
            half _Brightness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Get the object's position in world space
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                
                // Get camera basis vectors from UNITY_MATRIX_V
                float3 camRight = float3(UNITY_MATRIX_V._m00, UNITY_MATRIX_V._m10, UNITY_MATRIX_V._m20);
                float3 camUp = float3(UNITY_MATRIX_V._m01, UNITY_MATRIX_V._m11, UNITY_MATRIX_V._m21);
                float3 camForward = float3(UNITY_MATRIX_V._m02, UNITY_MATRIX_V._m12, UNITY_MATRIX_V._m22);
                
                // Extract scale from object's world matrix
                float3 objectScale = float3(
                    length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20)),
                    length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21)),
                    length(float3(UNITY_MATRIX_M._m02, UNITY_MATRIX_M._m12, UNITY_MATRIX_M._m22))
                );
                
                // Apply the object's scale to the vertex position
                float3 scaledVertex = float3(
                    input.positionOS.x * objectScale.x, 
                    input.positionOS.y * objectScale.y, 
                    0
                );
                
                // Check if we need to flip based on local scale X
                float flipFactor = 1.0;
                float3 worldRight = normalize(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20));
                float dotProduct = dot(worldRight, float3(1, 0, 0));
                
                if (dotProduct < -0.5)
                {
                    flipFactor = -1.0;
                }
                
                // Apply flip to X direction
                scaledVertex.x *= flipFactor;
                
                // Rotate the scaled vertex to face the camera
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex += scaledVertex.y * camUp;
                
                // Transform to clip space
                output.positionCS = TransformWorldToHClip(worldVertex);
                
                // Use the actual vertex world position for shadows and lighting
                output.positionWS = worldVertex;
                
                // Use camera forward as normal (billboard faces camera)
                output.normalWS = -camForward;
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                // Calculate shadow coordinate using the actual vertex position
                output.shadowCoord = TransformWorldToShadowCoord(worldVertex);
                
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
                Light mainLight = GetMainLight(input.shadowCoord);
                
                // Diffuse lighting with double-sided approach for billboard
                float NdotL = dot(input.normalWS, mainLight.direction);
                float NdotL_abs = abs(NdotL);
                half3 diffuse = mainLight.color * NdotL_abs;
                
                // Specular lighting (Blinn-Phong)
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float specular = pow(max(0.0, abs(dot(input.normalWS, halfDir))), _Glossiness * 256);
                half3 specColor = mainLight.color * specular * _Metallic;
                
                // Ambient lighting
                half3 ambient = SampleSH(input.normalWS);
                ambient = max(ambient, half3(0.1, 0.1, 0.1));
                
                // Use shadow attenuation from the light
                half shadowAtten = mainLight.shadowAttenuation;
                
                // Combine main lighting
                half3 lighting = ambient + (diffuse * shadowAtten) + (specColor * shadowAtten);
                
                // Additional lights (spot lights, point lights)
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                
                for (uint lightIndex = 0; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light additionalLight = GetAdditionalLight(lightIndex, input.positionWS);
                    half3 attenuatedLightColor = additionalLight.color * additionalLight.distanceAttenuation * additionalLight.shadowAttenuation;
                    
                    // Diffuse for additional light (abs for double-sided)
                    float addNdotL = abs(dot(input.normalWS, additionalLight.direction));
                    lighting += attenuatedLightColor * addNdotL;
                    
                    // Specular for additional light
                    float3 addHalfDir = normalize(additionalLight.direction + viewDir);
                    float addSpecular = pow(max(0.0, abs(dot(input.normalWS, addHalfDir))), _Glossiness * 256);
                    lighting += attenuatedLightColor * addSpecular * _Metallic;
                }
                #endif
                
                // Apply brightness multiplier
                lighting *= _Brightness;
                
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
                
                // Billboard in shadow caster
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                
                float3 camRight = float3(UNITY_MATRIX_V._m00, UNITY_MATRIX_V._m10, UNITY_MATRIX_V._m20);
                float3 camUp = float3(UNITY_MATRIX_V._m01, UNITY_MATRIX_V._m11, UNITY_MATRIX_V._m21);
                
                float3 objectScale = float3(
                    length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20)),
                    length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21)),
                    length(float3(UNITY_MATRIX_M._m02, UNITY_MATRIX_M._m12, UNITY_MATRIX_M._m22))
                );
                
                float3 scaledVertex = float3(
                    input.positionOS.x * objectScale.x, 
                    input.positionOS.y * objectScale.y, 
                    0
                );
                
                float flipFactor = 1.0;
                float3 worldRight = normalize(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20));
                float dotProduct = dot(worldRight, float3(1, 0, 0));
                
                if (dotProduct < -0.5)
                    flipFactor = -1.0;
                
                scaledVertex.x *= flipFactor;
                
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex += scaledVertex.y * camUp;
                
                output.positionCS = TransformWorldToHClip(worldVertex);
                output.uv = input.uv;
                
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
