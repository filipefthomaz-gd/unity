Shader "Custom/FullCameraFX/StripesMaskShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StripesColor ("StripeColor", Color) = (1,1,1,1)
        _Frequency ("Frequency", float) = 20
        _YSpeed ("YSpeed", float) = 10
        _Fill ("Fill", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
    Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag fullforwardshadows alpha:fade
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
            fixed4 _StripesColor;
            float _Frequency;
            float _YSpeed;
            float _Fill;
            
 
            float random (float2 input) { 
                return frac(sin(dot(input, float2(12.9898,78.233)))* 43758.5453123);
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                float stripes = 1 - step(_Fill, random( floor((i.uv.y+fmod(_Time.y, 1)*_YSpeed) * _Frequency)));
                fixed4 col = float4(stripes, stripes, stripes, 0);
                if(stripes < 1)
                    col = tex2D(_MainTex, i.uv);
                else
                    col = _StripesColor;

                return col;
            }
            ENDCG
        }
    }
}