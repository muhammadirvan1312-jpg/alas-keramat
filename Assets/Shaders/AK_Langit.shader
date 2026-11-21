Shader "AlasKeramat/Langit"
{
    Properties
    {
        _DayTex ("Langit Siang HDR", 2D) = "white" {}
        _NightTex ("Langit Malam HDR", 2D) = "black" {}
        _NightBlend ("Paduan Malam", Range(0,1)) = 0
        _DuskTex ("Langit Senja HDR", 2D) = "black" {}
        _DuskBlend ("Paduan Senja", Range(0,1)) = 0
        _Tint ("Warna", Color) = (1,1,1,1)
        _Exposure ("Eksposur", Range(0,4)) = 1.0
        _Rotation ("Rotasi Derajat", Range(0,360)) = 0
        _HorizonColor ("Warna Cakrawala", Color) = (0.55,0.68,0.72,1)
        _HorizonHeight ("Tinggi Kabut Cakrawala", Range(0.01,0.5)) = 0.16
        _SunDir ("Arah Matahari", Vector) = (0,0.5,0.5,0)
        _SunColor ("Warna Matahari", Color) = (1,0.92,0.78,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_DayTex); SAMPLER(sampler_DayTex);
            TEXTURE2D(_NightTex); SAMPLER(sampler_NightTex);
            TEXTURE2D(_DuskTex); SAMPLER(sampler_DuskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _DayTex_ST;
                half4 _Tint, _HorizonColor, _SunColor;
                float4 _SunDir;
                float _NightBlend, _DuskBlend, _Exposure, _Rotation, _HorizonHeight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dirOS : TEXCOORD0;
            };

            float3 RotateY(float3 d, float deg)
            {
                float rad = deg * 0.0174532925;
                float s, c;
                sincos(rad, s, c);
                return float3(d.x * c + d.z * s, d.y, -d.x * s + d.z * c);
            }

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dirOS = v.positionOS.xyz;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float3 rawDir = normalize(i.dirOS);
                float3 dir = RotateY(rawDir, _Rotation);
                float2 uv;
                uv.x = frac(atan2(dir.z, dir.x) * 0.159154943 + 0.5);
                uv.y = clamp(asin(clamp(dir.y, -1.0, 1.0)) * 0.318309886 + 0.5, 0.001, 0.999);

                half3 dayCol = SAMPLE_TEXTURE2D_LOD(_DayTex, sampler_DayTex, uv, 0).rgb;
                half3 nightCol = SAMPLE_TEXTURE2D_LOD(_NightTex, sampler_NightTex, uv, 0).rgb;
                half3 duskCol = SAMPLE_TEXTURE2D_LOD(_DuskTex, sampler_DuskTex, uv, 0).rgb;
                half3 sky = lerp(lerp(dayCol, nightCol, (half)_NightBlend), duskCol, (half)_DuskBlend) * _Tint.rgb * (half)_Exposure;

                float3 sunD = normalize(_SunDir.xyz + float3(0, 0.0001, 0));
                half sunDot = saturate((half)dot(rawDir, sunD));
                half sunDisc = pow(sunDot, 220.0h) * 2.4h + pow(sunDot, 16.0h) * 0.16h;
                sky += _SunColor.rgb * sunDisc * saturate((half)(rawDir.y * 4.0 + 0.25));

                half horizonMix = smoothstep((half)_HorizonHeight, -0.03h, (half)rawDir.y);
                half3 col = lerp(sky, _HorizonColor.rgb, horizonMix);
                return half4(max(col, 0.0h), 1.0h);
            }
            ENDHLSL
        }
    }
    Fallback "Skybox/Procedural"
}
