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
 
            struct Varyings
            {
                float4 position : SV_Position;
                float4 color : COLOR;
            };

            struct VertexData
            {
                float4 position;
                float4 color;
            };

            StructuredBuffer<VertexData> vertices;

            Varyings Vertex(uint vid : SV_VertexID)
            {
                VertexData v = vertices[vid];
                Varyings o;

                o.position = UnityObjectToClipPos(v.position);
                o.color = v.color;
                
                return o;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                return input.color;
            }

            ENDCG
        }
    }
}
