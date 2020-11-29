Shader "Custom/Vertex"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
 
            struct VertexData
            {
                float4 position : SV_Position;
                float4 color : COLOR;
            };

            StructuredBuffer<VertexData> vertices;

            VertexData vert(uint vid : SV_VertexID)
            {
                VertexData v = vertices[vid];
                
                v.position = UnityObjectToClipPos(v.position);

                return v;
            }

            float4 frag(VertexData input) : SV_Target
            {
                return input.color;
            }

            ENDCG
        }
    }
}
