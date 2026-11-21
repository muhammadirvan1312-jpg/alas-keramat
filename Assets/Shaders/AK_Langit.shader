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

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float FbmAwan(float2 p)
            {
                float v = 0.0;
                v += 0.55 * Noise2D(p);
                v += 0.30 * Noise2D(p * 2.07 + 13.7);
                v += 0.15 * Noise2D(p * 4.19 + 29.1);
                return v;
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

                // Bulan sabit/penuh di sisi berlawanan matahari dan bintang berkelip saat malam.
                float3 moonD = normalize(float3(-sunD.x, max(sunD.y, 0.18), -sunD.z));
                half moonDot = saturate((half)dot(rawDir, moonD));
                half moonDisc = smoothstep(0.9987h, 0.9993h, moonDot);
                half moonHalo = pow(moonDot, 48.0h) * 0.22h;
                half atasMask = saturate((half)(rawDir.y * 5.0));
                sky += half3(0.78h, 0.88h, 1.0h) * (moonDisc * 1.65h + moonHalo) * (half)_NightBlend * atasMask;

                float starCell = Hash21(floor(rawDir.xz / max(rawDir.y + 0.15, 0.15) * 95.0));
                float starTwinkle = 0.65 + 0.35 * sin(_Time.y * 2.4 + starCell * 62.0);
                half bintang = step(0.9945, starCell) * (half)starTwinkle * saturate((half)((rawDir.y - 0.06) * 6.0)) * (half)_NightBlend;

                // Awan prosedural berlapis bergerak pelan di kubah atas.
                float2 awanUV = rawDir.xz / max(rawDir.y + 0.18, 0.18) * 1.15 + float2(_Time.y * 0.0045, _Time.y * 0.0022);
                float nAwan = FbmAwan(awanUV);
                half awanAlpha = smoothstep(0.47, 0.74, nAwan) * saturate((half)((rawDir.y - 0.02) * 5.0)) * 0.68h;
                half3 awanSiang = lerp(half3(0.76h, 0.81h, 0.86h), half3(1.0h, 0.97h, 0.90h), pow(sunDot, 3.0h));
                half3 awanSenja = half3(0.96h, 0.56h, 0.36h);
                half3 awanMalam = half3(0.11h, 0.15h, 0.22h);
                half3 awanCol = lerp(lerp(awanSiang, awanMalam, (half)_NightBlend), awanSenja, (half)_DuskBlend);
                sky += half3(0.85h, 0.92h, 1.0h) * bintang * (1.0h - awanAlpha);
                sky = lerp(sky, awanCol, awanAlpha);

                half horizonMix = smoothstep((half)_HorizonHeight, -0.03h, (half)rawDir.y);
                half3 col = lerp(sky, _HorizonColor.rgb, horizonMix);
                return half4(max(col, 0.0h), 1.0h);
            }
            ENDHLSL
        }
    }
    Fallback "Skybox/Procedural"
}
