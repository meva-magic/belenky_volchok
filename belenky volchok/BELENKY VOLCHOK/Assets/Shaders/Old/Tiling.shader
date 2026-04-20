Shader "Custom/StandardWorldSpaceTiling"
{
    Properties
    {
        // Основные свойства стандартного шейдера
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        // Дополнительные свойства для управления текстурированием
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        
        // Параметр для настройки размера текстуры в мире (чем больше значение, тем чаще повтор)
        _WorldScale ("World Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Используем стандартную модель освещения
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;

        struct Input
        {
            // Вместо стандартных UV-координат будем использовать позицию в мире
            float3 worldPos;
            // Это нужно для работы системы автоматического тайлинга и оффсета в редакторе
            float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _BumpScale;
        float _WorldScale;

        // Встроенная функция Unity для трансформации текстурных координат
        float4 _MainTex_ST;
        float4 _BumpMap_ST;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Создаем UV-координаты на основе позиции в мире
            // Делим на _WorldScale, чтобы контролировать частоту повторения
            float2 worldUV = IN.worldPos.xy / _WorldScale;
            
            // Применяем стандартные настройки Tiling и Offset из материала
            // Это позволит настраивать повторяемость прямо в редакторе
            float2 mainUV = TRANSFORM_TEX(worldUV, _MainTex);
            float2 bumpUV = TRANSFORM_TEX(worldUV, _BumpMap);

            // Сэмплируем текстуры с новыми координатами
            fixed4 c = tex2D (_MainTex, mainUV) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            // Обрабатываем карту нормалей
            fixed4 normalTex = tex2D(_BumpMap, bumpUV);
            o.Normal = UnpackScaleNormal(normalTex, _BumpScale);
        }
        ENDCG
    }
    FallBack "Diffuse"
}