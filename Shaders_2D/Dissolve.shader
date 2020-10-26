// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/2D/Dissolve"
{
    Properties {
        [PerRendererData] _MainTex ("Main texture", 2D) = "white" {}
        _DissolveTex ("Dissolution texture", 2D) = "gray" {}
        _Threshold ("Threshold", Range(0., 1.01)) = 0.
        _RendererColor ("RendererColor", Color) = (1,1,1,1)
        _White ("WhiteColor", Color) = (0,0,0,1)
        _Epsilon("Epsilon", float) = 0.05
    }

    SubShader {

        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass {
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float4 color: COLOR;
                float2 uv : TEXCOORD0;
                
            };
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            fixed4 _RendererColor;
            fixed4 _White;
            float _Epsilon;

            v2f vert(appdata_t v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }

            sampler2D _DissolveTex;
            float _Threshold;

            fixed4 frag(v2f i) : SV_Target {
            
               
                
                float4 c = tex2D(_MainTex, i.uv) * _RendererColor;
                float val = tex2D(_DissolveTex, i.uv).a; //Red Value of noise
                
                
                //Changes color of sprite
                /*if ( c.a >= 0.1){
                    c = c + (i.color - c) * i.color.a * c.a;
                }*/
                
                
                if ( c.a >= 0.1){
                    c = c*i.color* i.color.a * c.a;
                }
                
                //If the Red value of the dissolve texture is slightly larger than the threshold, turn white (glowy)
                if (val < _Threshold+_Epsilon)
                {
                     c = _White;
                }
            
                c.rgb *= c.a;
                c.a *= step(_Threshold, val);
                
                return c;
            }
            ENDCG
        }
    }
}

