Shader "Unlit/WaterEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaterColor ("Water Color", Color) = (0,0.5,1,0.5)
        _Speed ("Speed", Range(0, 10)) = 1
        _Distortion ("Distortion", Range(0, 1)) = 0.1
        _Frequency ("Frequency", Range(1, 50)) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

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
            float4 _MainTex_ST;
            fixed4 _WaterColor;
            float _Speed;
            float _Distortion;
            float _Frequency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 distortedUV = i.uv;
                distortedUV.x += sin(i.uv.y * _Frequency + _Time.y * _Speed) * _Distortion;
                distortedUV.y += cos(i.uv.x * _Frequency + _Time.y * _Speed) * _Distortion;

                fixed4 col = tex2D(_MainTex, distortedUV);
                col *= _WaterColor;
                return col;
            }
            ENDCG
        }
    }
}