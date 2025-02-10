Shader "Custom/TextureBlend"
{
    Properties
    {
        _TextureR ("Texture R", 2D) = "white" {}
        _TextureG ("Texture G", 2D) = "white" {}
        _TextureB ("Texture B", 2D) = "white" {}
        _TextureA ("Texture A", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _TextureR;
        sampler2D _TextureG;
        sampler2D _TextureB;
        sampler2D _TextureA;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 texR = tex2D(_TextureR, IN.uv_MainTex) * IN.color.r;
            fixed4 texG = tex2D(_TextureG, IN.uv_MainTex) * IN.color.g;
            fixed4 texB = tex2D(_TextureB, IN.uv_MainTex) * IN.color.b;
            fixed4 texA = tex2D(_TextureA, IN.uv_MainTex) * IN.color.a;

            o.Albedo = texR.rgb + texG.rgb + texB.rgb + texA.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
