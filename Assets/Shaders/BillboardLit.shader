Shader "URP/SimpleBillboard_Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                
                // Get camera basis vectors
                float3 camRight = unity_MatrixV[0].xyz;
                float3 camUp = unity_MatrixV[1].xyz;
                
                // Extract scale
                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                
                float3 scaledVertex = float3(v.vertex.x * objectScale.x, v.vertex.y * objectScale.y, 0);
                
                // Billboard positioning (no flip)
                float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex += scaledVertex.y * camUp;
                
                o.pos = mul(UNITY_MATRIX_VP, float4(worldVertex, 1.0));
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

            Varyings vert(Attributes v)
            {
                Varyings o;
                
                // Get object position in world space
                float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Get camera basis vectors
                float3 camRight = unity_MatrixV[0].xyz;
                float3 camUp = unity_MatrixV[1].xyz;
                
                // Extract scale
                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                
                float3 scaledVertex = float3(v.vertex.x * objectScale.x, v.vertex.y * objectScale.y, 0);
                
                // Billboard rotation (no flip)
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex += scaledVertex.y * camUp;
                
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Billboard normal always faces camera
                float3 camForward = unity_MatrixV[2].xyz;
                o.worldNormal = -camForward;
                o.worldPos = worldVertex;
                
                o.fogCoord = ComputeFogFactor(o.vertex.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
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
