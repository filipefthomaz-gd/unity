Shader "Custom/Replacement/Overdraw"
{
    Properties
    {
        _OverDrawColor("OverdrawColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }

        ZTest Always
        ZWrite Off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
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
                return o;
            }

            half4 _OverDrawColor;

            fixed4 frag (v2f i) : SV_Target
            {
                return _OverDrawColor;
            }
            ENDCG
        }
    }

    
}
