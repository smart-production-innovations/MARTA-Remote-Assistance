Shader "Hidden/WebcamBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_scaleX ("Scale X", Float) = 1.0
		_scaleY ("Scale Y", Float) = 1.0
		_rotationAngle("Rotation Angle", Int) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float _scaleX;
			float _scaleY;
			int _rotationAngle;

            fixed4 frag (v2f i) : SV_Target
            {
				float2 uv;
                //rotate the texture
				switch (_rotationAngle) 
				{
				case 0:
				default:
					uv = float2(i.uv.x, i.uv.y);
					break;
				case 90:
					uv = float2(1 - i.uv.y, i.uv.x);
					break;
				case 180:
					uv = float2(1 - i.uv.x, 1 - i.uv.y);
					break;
				case 270:
					uv = float2(i.uv.y, 1 - i.uv.x);
					break;
				}
				
				uv = 2*uv - 1.0f;
                //scale the texture
				uv *= float2(_scaleX, _scaleY);
				uv = (1 + uv) * 0.5f;

				fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}
