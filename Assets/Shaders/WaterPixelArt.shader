Shader "Custom/WaterPixelArt2D"
{
    Properties
    {
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.0
        _WaveStrength ("Wave Strength", Range(0, 0.05)) = 0.01
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.3
        _WaterColor ("Water Color", Color) = (0.2, 0.6, 1.0, 0.8)
        _ReflectionTint ("Reflection Tint", Color) = (0.8, 0.9, 1.0, 1.0)
        _PixelSize ("Pixel Size", Range(1, 20)) = 4
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "WaterPass"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _WaveSpeed;
                float _WaveStrength;
                float _ReflectionStrength;
                float4 _WaterColor;
                float4 _ReflectionTint;
                float _PixelSize;
            CBUFFER_END
            
            float2 PixelSnap(float2 pos, float pixelSize)
            {
                return floor(pos * pixelSize) / pixelSize;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                output.positionHCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.worldPos = vertexInput.positionWS.xy;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 pixelWorldPos = PixelSnap(input.worldPos, _PixelSize);
                
                float time = _Time.y * _WaveSpeed;
                
                float2 wave1 = float2(
                    sin(time + pixelWorldPos.x * 2.0), 
                    cos(time * 0.8 + pixelWorldPos.y * 1.5)
                ) * _WaveStrength;
                
                float2 wave2 = float2(
                    cos(time * 1.3 + pixelWorldPos.y * 3.0), 
                    sin(time * 0.7 + pixelWorldPos.x * 2.5)
                ) * _WaveStrength * 0.6;
                
                float2 distortedPos = pixelWorldPos + wave1 + wave2;
                
                float wavePattern = sin(time * 2.0 + distortedPos.x * 4.0) * 
                                  cos(time * 1.5 + distortedPos.y * 3.0) * 0.1 + 0.9;
                
                float reflection = sin(time * 3.0 + distortedPos.x * 1.5) * 
                                 cos(time * 2.5 + distortedPos.y * 2.0) * 0.2 + 0.8;
                
                half3 waterBase = _WaterColor.rgb * wavePattern;
                half3 reflectionColor = _ReflectionTint.rgb * reflection * _ReflectionStrength;
                
                half3 finalColor = lerp(waterBase, waterBase + reflectionColor, 0.5);
                
                float alpha = _WaterColor.a;
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Sprites/Default"
}