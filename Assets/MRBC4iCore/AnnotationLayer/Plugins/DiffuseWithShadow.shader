// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader"Custom/DiffuseWithShadow"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_ShadowMap("Shadow Map", 2D) = "white" {}
		_Angle ("Angle", Float) = 0.0
		_Hue("Hue", Range(-360, 360)) = 0.
		_Brightness("Brightness", Range(-1, 1)) = 0.
		_Contrast("Contrast", Range(0, 2)) = 1
		_Saturation("Saturation", Range(0, 2)) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200

		Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
			#pragma exclude_renderers gles

			#pragma vertex vert
			#pragma fragment frag alpha

			uniform sampler2D _MainTex;
			uniform sampler2D _ShadowMap;
			uniform float4x4 _LightViewProj;
			uniform float _Angle;
			float _Hue;
			float _Brightness;
			float _Contrast;
			float _Saturation;
			float _near;


			struct VertexOut
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float4 proj : TEXCOORD1;
			};

			struct PixelOut
			{
				float4	color : COLOR;
			};

			VertexOut vert( float4 position : POSITION, float2 uv : TEXCOORD0 )
			{
				VertexOut OUT;

				OUT.position =  UnityObjectToClipPos( position );
				OUT.uv = uv;

				//OUT.proj = mul(mul(unity_ObjectToWorld, float4(position.xyz, 1.7)), _LightViewProj);
				//OUT.proj = mul(mul(unity_ObjectToWorld, float4(position.xyz, 1)), _LightViewProj);
				//OUT.proj = mul(mul(unity_ObjectToWorld, float4(position.xyz, 1.5)), _LightViewProj);
				OUT.proj = mul(mul(unity_ObjectToWorld, float4(position.xyz, 1)), _LightViewProj);

				return OUT;
			}

			inline float3 applyHue(float3 aColor, float aHue)
			{
				float angle = radians(aHue);
				float3 k = float3(0.57735, 0.57735, 0.57735);
				float cosAngle = cos(angle);
				//Rodrigues' rotation formula
				return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
			}

			inline float4 applyHSBEffect(float4 startColor)
			{
				float4 outputColor = startColor;
				outputColor.rgb = applyHue(outputColor.rgb, _Hue);
				outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast)+0.5f;
				outputColor.rgb = outputColor.rgb + _Brightness;
				float3 intensity = dot(outputColor.rgb, float3(0.299, 0.587, 0.114));
				outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
				return outputColor;
			}

			PixelOut frag(VertexOut IN)
			{
				PixelOut OUT;

				float2 ndc = float2(IN.proj.x/IN.proj.w, IN.proj.y/IN.proj.w);
				//float2 uv = (1.7 + float2( ndc.x, ndc.y)) * 0.3;// * 0.5;
				//float2 uv = (0.85 + float2( ndc.x, ndc.y)) * 0.6;// * 0.5;
				//float2 uv = (0.5 + float2( ndc.x, ndc.y)) * 1;// * 0.5;

				//_near = 0.12;
				float _offset = 0.5 * 1 / _near;
				float2 uv = (_offset + float2( ndc.x, ndc.y)) * _near;

				float theta = _Angle * 3.14159 / 180;
				float2x2 matRot = float2x2( cos(theta), sin(theta),
											-sin(theta), cos(theta) );
				uv = mul( uv, matRot);

				float4 c = tex2D( _ShadowMap, uv );

				if( uv.x < 0 || uv.y < 0 ||
					uv.x > 1  || uv.y > 1 || c.a <= 0.00f )
					{
						c = tex2D(_MainTex, IN.uv);
					}

				c = applyHSBEffect(c);
				OUT.color = c;

				return OUT;
			}

			ENDCG
		}

	} 
	FallBack"Diffuse"
}


