Shader "Custom/SurfaceBuffers" {
	Properties {
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		Cull Off

		CGPROGRAM

		#pragma surface surf Standard vertex:vert addshadow
		#pragma instancing_options procedural:setup
		#pragma target 3.5

		#if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL) || defined(SHADER_API_SWITCH) || defined(SHADER_API_VULKAN) || (defined(SHADER_API_METAL) && defined(UNITY_COMPILER_HLSLCC)))
			#define SUPPORT_STRUCTUREDBUFFER
		#endif

		#if (defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)) && defined(SUPPORT_STRUCTUREDBUFFER)
			#define ENABLE_INSTANCING
		#endif

		half _Glossiness;
		half _Metallic;
		float4x4 _LocalToWorld;
		float4x4 _WorldToLocal;

#ifdef ENABLE_INSTANCING
		struct CustomVertex {
			float3 position;
			float4 color;
		};

		StructuredBuffer<CustomVertex> vertexBuffer;
#endif

		struct Input {
			float4 color;
		};

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			uint vid : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		void setup()
		{
			unity_ObjectToWorld = _LocalToWorld;
			unity_WorldToObject = _WorldToLocal;
		}
		
		void vert(inout appdata v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

#ifdef ENABLE_INSTANCING
			v.vertex = float4(vertexBuffer[v.vid].position, 1);
			o.color = vertexBuffer[v.vid].color;
#else
			v.vertex = float4(0, 0, 0, 1);
			o.color = float4(1, 0, 0, 1);
#endif

			v.normal = normalize(ObjSpaceViewDir(v.vertex));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 c = IN.color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
