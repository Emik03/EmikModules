Shader "KT/Mobile/EmissiveTint" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Color ("Tint", Color) = (1,1,1,1)
	_Emissive ("Emissive", float) = 1
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;
float _Emissive;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	o.Emission = c.rgb * c.a * _Emissive;
}
ENDCG
}

Fallback "KT/Mobile/DiffuseTint"
}
