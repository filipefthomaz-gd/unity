// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FX/Mirror2"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[HideInInspector] _ReflectionTex ("", 2D) = "white" {}
        
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _WaveSpeed("Wave Speed", float) = 1.0
        _WaveAmp("Wave Amp", float) = 0.2
        
        _DistortionScrollX("X Scroll Speed", Range(-0.1,0.1)) = -0.1
        _DistortionScrollY("Y Scroll Speed", Range(-0.1,0.1)) = 0.1
        
        _DistortionScaleX("X Scale", float) = 1.0
        _DistortionScaleY("Y Scale", float) = 1.0
        
        
	}
	SubShader
	{
		Tags  { "RenderType"="Opaque" }
		LOD 100
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
            
            sampler2D _NoiseTex;
            float  _WaveSpeed;
            float  _WaveAmp;

			struct appdata_t
			{
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : POSITION;
				float4 col: COLOR;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : SV_POSITION;
				fixed4 col: COLOR;
			};
			float4 _MainTex_ST;
			fixed4 _Color;

            
            
			v2f vert(appdata_t i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (i.pos);
				o.uv = i.uv.xy;
				o.col = i.col * _Color;
				o.refl = ComputeScreenPos (o.pos);


                // For Wave Animation (For now only on X)
                float noiseSample = tex2Dlod(_NoiseTex, float4(i.uv.xy, 0, 0));
                o.pos.x += cos(_Time*_WaveSpeed*noiseSample)*_WaveAmp;
              
               
                #if UNITY_UV_STARTS_AT_TOP
                float4 scale = -1.0;
                #else
                float4 scale = 1.0;
                #endif 
       
                
              
				return o;
			}
            
			sampler2D _MainTex;
			sampler2D _ReflectionTex;
            
            float _DistortionScrollX;
            float _DistortionScrollY;
            float _DistortionPower;
            float _DistortionScaleX;
            float _DistortionScaleY;
            
			fixed4 frag(v2f i) : SV_Target
			{
                float4 distortionScale = (_DistortionScaleX, _DistortionScaleY);
                float4 distortionScroll = (_DistortionScrollX, _DistortionScrollY);
                
				//fixed4 tex = tex2D(_MainTex, i.uv) * i.col;
                fixed4 tex = tex2D(_MainTex, i.uv + (tex2D(_NoiseTex, distortionScale * i.uv))) * i.col;
				tex.rgb *= tex.a;
				fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl-0.01*tex2D(_NoiseTex, distortionScale* i.refl+_Time*distortionScroll)));
                // - 0.01*(tex2D(_NoiseTex, distortionScale * i.refl))
				return tex * refl;
			}
           
            
			ENDCG
	    }
	}
}