// Original code source:
// http://www.kamend.com/2014/05/rendering-a-point-cloud-inside-unity/
// Modifications:
// - point size varying with distance

Shader "Custom/VertexColor" {
    SubShader {
    Pass {
        LOD 200
         
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"
 
        struct VertexInput {
            float4 v : POSITION;
            float4 color: COLOR;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
            float size : PSIZE;
        };
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            o.pos = UnityObjectToClipPos(v.v);
            o.col = float4(v.color.r, v.color.g, v.color.b  , 1.0f);
            o.size = 1.0; //disable size computation for now
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return o.col;
        }
 
        ENDCG
        } 
    }
}

