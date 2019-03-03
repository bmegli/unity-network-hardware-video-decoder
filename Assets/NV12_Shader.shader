Shader "Unlit/NV12_Shader"
{
	Properties
	{
		_MainTex ("Texture Y", 2D) = "white" {}
		_UV ("Texture UV", 2D) = "gray" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			//app to vertex
			struct a2v
			{
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
			};

			//vertex to fragment
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;  //Y
			sampler2D _UV;

			v2f vert (a2v v)
			{
				v2f o = { v.uv, UnityObjectToClipPos(v.vertex) };
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed Y=tex2D(_MainTex, i.uv).r;
				fixed4 UV=tex2D(_UV, i.uv);
				fixed U=UV.r - 0.5;
				fixed V=UV.g - 0.5;

				//R, G, B
				fixed4 col = {Y + 1.402*V,  Y - 0.34414 * U - 0.71414 *V, Y + 1.772 * U, 1};
				//fixed4 col = {Y,  Y, Y, 1};

				return col;
			}
			ENDCG
		}
	}
}
