Shader "Custom/Vertex"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"
 
            struct Attributes
            {
                float3 position : POSITION;
            };

            struct Varyings
            {
                float4 position : SV_Position;
                half3 color : COLOR;
            };

            StructuredBuffer<float3> vertices;

            Varyings Vertex(uint vid : SV_VertexID)
            {
                float3 pt = vertices[vid];
                half3 col = half3(1.0, 1.0, 1.0);

                Varyings o;
                o.position = UnityObjectToClipPos(pt);
                o.color = col;

                return o;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return half4(input.color, 1.0);
            }

            ENDCG
        }
    }
}
