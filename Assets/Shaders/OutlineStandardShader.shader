// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//http://www.shaderslab.com/demo-19---outline-3d-model.html
//DIMI Standard Shader variation
Shader "Custom/OutlineSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
			_Outline("_Outline", Range(0,0.1)) = 0
			_OutlineColor("Color", Color) = (1, 1, 1, 1)
	}
	SubShader {

			Pass{
			Tags{ "RenderType" = "Opaque" }
			Cull Front

			CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

			struct v2f {
			float4 pos : SV_POSITION;
		};

		float _Outline;
		float4 _OutlineColor;

		float4 vert(appdata_base v) : SV_POSITION{
			v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		float3 normal = mul((float3x3) UNITY_MATRIX_MV, v.normal);
		normal.x *= UNITY_MATRIX_P[0][0];
		normal.y *= UNITY_MATRIX_P[1][1];
		o.pos.xy += normal.xy * _Outline;
		return o.pos;
		}

			half4 frag(v2f i) : COLOR{
			return _OutlineColor;
		}

			ENDCG
		}



		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
