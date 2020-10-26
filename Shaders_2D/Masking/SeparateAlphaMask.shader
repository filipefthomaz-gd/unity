Shader "Separate Alpha Mask" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Alpha ("Alpha (A)", 2D) = "white" {}
        _BlendFactor ("Blend Factor", Range(0., 1.0)) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
       
        ZWrite Off
       
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
       
        Pass {
            SetTexture[_MainTex] {
                Combine texture
            }
            SetTexture[_Alpha] {
                Combine previous, texture
            }
        }
    }
}