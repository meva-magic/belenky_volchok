Shader "Unlit/BillboardWithFlip"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle(FLIP_X)] _FlipX("Flip X Axis", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature FLIP_X

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Get the object's position in world space
                float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Get camera basis vectors (right, up, forward) in world space
                float3 camRight = UNITY_MATRIX_V[0].xyz;  // Camera right
                float3 camUp = UNITY_MATRIX_V[1].xyz;     // Camera up
                
                // Extract scale from object's world matrix
                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                
                // Apply the object's scale to the vertex position
                float3 scaledVertex = float3(v.vertex.x * objectScale.x, v.vertex.y * objectScale.y, 0);
                
                // Check if we need to flip based on local scale X
                // If localScale.x is negative, flip the X direction
                float flipFactor = 1.0;
                
                // Get the sign of the local scale from the object's transform
                // This is a bit hacky but works: check if the first column of world matrix is negative
                float3 worldRight = normalize(unity_ObjectToWorld._m00_m10_m20);
                float3 originalRight = float3(1, 0, 0);
                float dotProduct = dot(worldRight, originalRight);
                
                // If the world right direction is opposite to original right, flip
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
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Flip UVs if needed (optional - uncomment if textures appear mirrored)
                // if (flipFactor < 0)
                //     o.uv.x = 1.0 - o.uv.x;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - 0.01);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
