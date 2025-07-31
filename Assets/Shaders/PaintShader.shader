Shader "Custom/PaintShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _UV ("UV", Vector) = (0,0,0,0)
        _Size ("Brush Size", Float) = 0.1
        _Opacity ("Opacity", Range(0,1)) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _UV;
            float _Size;
            float _Opacity;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
                float2 brushUV = _UV.xy;

                float dist = distance(uv, brushUV);
                float falloff = smoothstep(_Size, 0.0, dist);

                fixed4 baseColor = tex2D(_MainTex, uv);             // read existing pixel
                fixed4 paintColor = fixed4(_Color.rgb, 1.0);         // new color

                fixed4 blended = lerp(baseColor, paintColor, falloff * _Opacity);
                return blended;
            }


            ENDCG
        }
    }
}
