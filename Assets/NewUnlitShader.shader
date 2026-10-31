Shader "Custom/SkyboxBlend"
{
    Properties
    {
        _Sky1 ("Sky 1 (Day)", Cube) = "" {}
        _Sky2 ("Sky 2 (Evening)", Cube) = "" {}
        _Sky3 ("Sky 3 (Night)", Cube) = "" {}
        _Blend12 ("Blend 1->2", Range(0,1)) = 0
        _Blend23 ("Blend 2->3", Range(0,1)) = 0
        _Intensity ("Intensity", Range(0,2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" }
        Pass
        {
            ZWrite Off
            Cull Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 uv : TEXCOORD0;
            };

            samplerCUBE _Sky1;
            samplerCUBE _Sky2;
            samplerCUBE _Sky3;
            float _Blend12;
            float _Blend23;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Сначала смешиваем Sky1 и Sky2
                fixed4 sky12 = lerp(texCUBE(_Sky1, i.uv), texCUBE(_Sky2, i.uv), _Blend12);
                
                // Потом смешиваем результат с Sky3
                fixed4 sky123 = lerp(sky12, texCUBE(_Sky3, i.uv), _Blend23);
                
                return sky123 * _Intensity;
            }
            ENDCG
        }
    }
}