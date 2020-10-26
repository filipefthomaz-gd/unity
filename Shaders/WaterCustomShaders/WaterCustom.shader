Shader "Custom/WaterCustom"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _ShoreIntersectionThreshold("Shore intersection threshold", float) = 0
    }
    SubShader
    {       
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True"}
        Blend One OneMinusSrcAlpha
        ZWrite Off
        LOD 200


        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _CameraDepthTexture;

        struct Input
        {
            float4 color: Color;
            float3 worldPos;
            float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float smootherstep(float x) {
            x = saturate(x);
            return saturate(x * x * x * (x * (6 * x - 15) + 10));
        }
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float _ShoreIntersectionThreshold;


        void vert(inout appdata_full v) {
            float4 v0 = v.vertex;
            
            float4 screenPos = ComputeScreenPos(UnityObjectToClipPos(v0.xyz));

            v.vertex = v0;

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            depth = LinearEyeDepth(depth);
            float shoreDiff = smootherstep(saturate((depth - IN.screenPos.w) / _ShoreIntersectionThreshold));
            // Albedo comes from a texture tinted by color
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = lerp(1.0, 0.5, 1-shoreDiff);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
