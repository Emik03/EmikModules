// As long as _W2L is assigned every frame, the color will be based on localPosition.

Shader "KT/SphereShader"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
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
                float4 localPos : TEXCOORD1;
            };

            float4x4 _W2L;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.localPos = mul(_W2L, mul(unity_ObjectToWorld, float4(0, 0, 0, 1)));

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = i.localPos;
                return float4(
                    (col.r + 0.05) * 7.5 * _Color.r,
                    (col.g - 0.1) * 7.5 * _Color.g, 
                    (col.b + 0.05) * 7.5 * _Color.b, 
                    col.a);
            }
            ENDCG
        }
    }
}