Shader "Hidden/CrtPostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _UTime("Time", float) = 1
         _bend("Bend", float) = 1
         _scanline_size_1("Scansize1", float) = 1
         _scanline_speed_1("Scanspeed1", float) = 1
         _scanline_size_2("Scansize2", float) = 1
         _scanline_speed_2("Scanspeed2", float) = 1
         _scanline_amount("ScanAmount", float) = 1
         _vignette_size("VignSize", float) = 1
         _vignette_smoothness("VignSmooth", float) = 1
         _vignette_edge_round("VignEdgeRound", float) = 1
         _noise_size("NoiseSize", float) = 1
         _noise_amount("NoiseAmount", float) = 1
         _red_offset("RedOffset", Vector) = (1,1,0,0)
         _blue_offset("BlueOffset", Vector) = (1,1,0,0)
         _green_offset("GreenOffset", Vector) = (1,1,0,0)
	}
	SubShader
	{
        Tags { "Queue" = "Transparent" }
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

        GrabPass
        {
            "_GrabTexture"
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
				o.uv = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			
			sampler2D _MainTex;

			float _UTime;
			float _bend;
			float _scanline_size_1;
			float _scanline_speed_1;
			float _scanline_size_2;
			float _scanline_speed_2;
			float _scanline_amount;
			float _vignette_size;
			float _vignette_smoothness;
			float _vignette_edge_round;
			float _noise_size;
			float _noise_amount;
			half2 _red_offset;
			half2 _green_offset;
			half2 _blue_offset;

			half2 crt_coords(half2 uv, float bend)
			{
				uv -= 0.5;
				uv *= 2.;
				uv.x *= 1. + pow(abs(uv.y) / bend, 2.);
				uv.y *= 1. + pow(abs(uv.x) / bend, 2.);

				uv /= 2.5;
				return uv + 0.5;
			}

			float vignette(half2 uv, float size, float smoothness, float edgeRounding)
			{
				uv -= .5;
				uv *= size;
				float amount = sqrt(pow(abs(uv.x), edgeRounding) + pow(abs(uv.y), edgeRounding));
				amount = 1. - amount;
				return smoothstep(0, smoothness, amount);
			}

			float scanline(half2 uv, float lines, float speed)
			{
				return sin(uv.y * lines + _Time.y * speed);
			}

			float random(half2 uv)
			{
				return frac(sin(dot(uv, half2(15.1511, 42.5225))) * 12341.51611 * sin(_Time.y * 0.03));
			}

			float noise(half2 uv)
			{
				half2 i = floor(uv);
				half2 f = frac(uv);

				float a = random(i);
				float b = random(i + half2(1., 0.));
				float c = random(i + half2(0, 1.));
				float d = random(i + half2(1., 1.));

				half2 u = smoothstep(0., 1., f);

				return lerp(a, b, u.x) + (c - a) * u.y * (1. - u.x) + (d - b) * u.x * u.y;
			}


            sampler2D _GrabTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				//half2 crt_uv = crt_coords(i.uv, _bend);
				fixed4 col;
				col.r = tex2D(_GrabTexture, i.uv + _red_offset).r;
				col.g = tex2D(_GrabTexture, i.uv + _green_offset).g;
				col.b = tex2D(_GrabTexture, i.uv + _blue_offset).b;
				col.a = tex2D(_GrabTexture, i.uv).a;

				float s1 = scanline(i.uv, _scanline_size_1, _scanline_speed_1);
				float s2 = scanline(i.uv, _scanline_size_2, _scanline_speed_2);

				col = lerp(col, fixed(s1 + s2), _scanline_amount);

				return lerp(col, fixed(noise(i.uv * _noise_size)), _noise_amount) * vignette(i.uv, _vignette_size, _vignette_smoothness, _vignette_edge_round);

                //return col;
			}
			ENDCG
		}
	}
}