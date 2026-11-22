Shader "AlasKeramat/Langit"
{
    Properties
    {
        _DayTex ("Langit Siang HDR", 2D) = "white" {}
        _NightTex ("Langit Malam HDR", 2D) = "black" {}
        _NightBlend ("Paduan Malam", Range(0,1)) = 0
        _DuskTex ("Langit Senja HDR", 2D) = "black" {}
        _DuskBlend ("Paduan Senja", Range(0,1)) = 0
        _StormTex ("Langit Badai HDR", 2D) = "black" {}
        _StormBlend ("Paduan Badai", Range(0,1)) = 0
        _CloudCover ("Tebal Awan", Range(0,1)) = 0.52
        _Lightning ("Kilatan Petir", Range(0,1)) = 0
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
            TEXTURE2D(_StormTex); SAMPLER(sampler_StormTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _DayTex_ST;
                half4 _Tint, _HorizonColor, _SunColor;
                float4 _SunDir;
                float _NightBlend, _DuskBlend, _StormBlend, _CloudCover, _Lightning;
                float _Exposure, _Rotation, _HorizonHeight;
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
                v += 0.52 * Noise2D(p);
                v += 0.30 * Noise2D(p * 2.07 + 13.7);
                v += 0.18 * Noise2D(p * 4.23 + 29.1);
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
                half3 stormCol = SAMPLE_TEXTURE2D_LOD(_StormTex, sampler_StormTex, uv, 0).rgb;

                half3 sky = lerp(lerp(dayCol, nightCol, (half)_NightBlend), duskCol, (half)_DuskBlend);
                // Bila _StormTex terpasang (tidak hitam penuh), padukan HDRI badai 4K secara mulus.
                half stormAda = saturate(dot(stormCol, half3(1.0h, 1.0h, 1.0h)) * 4.0h);
                sky = lerp(sky, stormCol * lerp(1.0h, 0.35h, (half)_NightBlend), (half)_StormBlend * stormAda * 0.85h);
                sky *= _Tint.rgb * (half)_Exposure;

                float3 sunD = normalize(_SunDir.xyz + float3(0, 0.0001, 0));
                half sunDot = saturate((half)dot(rawDir, sunD));
                half cerahMask = 1.0h - (half)_StormBlend * 0.78h;
                half sunDisc = (pow(sunDot, 240.0h) * 2.6h + pow(sunDot, 14.0h) * 0.22h) * cerahMask;
                sky += _SunColor.rgb * sunDisc * saturate((half)(rawDir.y * 4.0 + 0.25));

                // Bulan sabit/penuh, pita Bima Sakti tipis & bintang berkelip saat malam cerah.
                float3 moonD = normalize(float3(-sunD.x, max(sunD.y, 0.18), -sunD.z));
                half moonDot = saturate((half)dot(rawDir, moonD));
                half moonDisc = smoothstep(0.9987h, 0.9993h, moonDot);
                half moonHalo = pow(moonDot, 42.0h) * 0.26h;
                half atasMask = saturate((half)(rawDir.y * 5.0));
                sky += half3(0.78h, 0.88h, 1.0h) * (moonDisc * 1.75h + moonHalo) * (half)_NightBlend * atasMask * cerahMask;

                // Bintang melingkar tajam 2 lapis (tanpa piksel kotak!) & pita Bima Sakti halus saat malam cerah.
                float2 starProj = rawDir.xz / max(rawDir.y + 0.16, 0.16);
                float2 sGrid1 = starProj * 78.0;
                float2 sCell1 = floor(sGrid1), sFrac1 = frac(sGrid1);
                float sHash1 = Hash21(sCell1);
                float2 sOff1 = float2(frac(sHash1 * 17.3), frac(sHash1 * 31.7)) * 0.56 + 0.22;
                float sDist1 = length(sFrac1 - sOff1);
                float sTwink1 = 0.68 + 0.32 * sin(_Time.y * 2.8 + sHash1 * 62.0);
                half star1 = smoothstep(0.085, 0.012, sDist1) * step(0.86, sHash1) * (half)sTwink1;

                float2 sGrid2 = starProj * 155.0 + 19.3;
                float2 sCell2 = floor(sGrid2), sFrac2 = frac(sGrid2);
                float sHash2 = Hash21(sCell2);
                float2 sOff2 = float2(frac(sHash2 * 23.1), frac(sHash2 * 41.9)) * 0.6 + 0.2;
                half star2 = smoothstep(0.095, 0.015, length(sFrac2 - sOff2)) * step(0.88, sHash2) * 0.62h;

                half nightSkyMask = saturate((half)((rawDir.y - 0.04) * 6.0)) * (half)_NightBlend * cerahMask;
                half3 starTint = lerp(half3(0.82h, 0.90h, 1.0h), half3(1.0h, 0.92h, 0.78h), (half)frac(sHash1 * 9.7));
                half3 bintangCol = starTint * (star1 + star2) * nightSkyMask * 1.35h;

                half bimaMask = exp(-abs(rawDir.x * 1.45h - rawDir.z * 0.82h + 0.14h) * 3.6h);
                half bimaDust = (half)FbmAwan(starProj * 1.8 + 4.2);
                sky += half3(0.42h, 0.56h, 0.86h) * (bimaMask * bimaDust * atasMask * (half)_NightBlend * cerahMask * 0.09h);

                // Pendaran atmosfer hangat di cakrawala dekat matahari saat fajar & senja.
                half scatterH = pow(sunDot, 5.0h) * exp(-abs((half)rawDir.y) * 5.2h) * cerahMask;
                sky += _SunColor.rgb * scatterH * lerp(0.18h, 0.52h, (half)_DuskBlend);

                // Awan 2 lapis (sirus tinggi + kumulus tebal) dengan silver-lining matahari & kilatan petir badai.
                float2 proj = rawDir.xz / max(rawDir.y + 0.17, 0.17);
                float2 awanUV1 = proj * 1.12 + float2(_Time.y * 0.0048, _Time.y * 0.0024);
                float2 awanUV2 = proj * 2.25 - float2(_Time.y * 0.0025, -_Time.y * 0.0038);
                float n1 = FbmAwan(awanUV1);
                float n2 = Noise2D(awanUV2) * 0.45 + 0.55 * Noise2D(awanUV2 * 1.9 + 7.1);
                float nAwan = lerp(n1, n1 * 0.68 + n2 * 0.32, 0.55);

                half ambang = lerp(0.56h, 0.28h, saturate((half)_CloudCover));
                half awanAlpha = smoothstep(ambang, ambang + 0.26h, (half)nAwan) * saturate((half)((rawDir.y - 0.015) * 5.0)) * lerp(0.68h, 0.92h, (half)_StormBlend);
                half tebalInti = smoothstep(ambang + 0.06h, ambang + 0.32h, (half)nAwan);

                half silverLining = pow(sunDot, 7.0h) * (1.0h - tebalInti * 0.65h) * 0.55h * cerahMask;
                half3 awanSiang = lerp(half3(0.84h, 0.88h, 0.92h), half3(0.58h, 0.64h, 0.70h), tebalInti * 0.55h) + _SunColor.rgb * silverLining;
                half3 awanSenja = lerp(half3(1.0h, 0.66h, 0.40h), half3(0.48h, 0.22h, 0.24h), tebalInti * 0.7h) + half3(1.0h, 0.72h, 0.38h) * silverLining;
                half3 awanMalam = lerp(half3(0.12h, 0.16h, 0.24h), half3(0.05h, 0.07h, 0.12h), tebalInti);
                half3 awanBadai = lerp(half3(0.30h, 0.34h, 0.38h), half3(0.11h, 0.13h, 0.16h), tebalInti) * lerp(1.0h, 0.35h, (half)_NightBlend);

                half3 awanCol = lerp(lerp(awanSiang, awanMalam, (half)_NightBlend), awanSenja, (half)_DuskBlend);
                awanCol = lerp(awanCol, awanBadai, (half)_StormBlend);
                // Kilatan petir menerangi awan dari dalam saat badai!
                awanCol += half3(0.82h, 0.90h, 1.0h) * ((half)_Lightning * (0.45h + 0.85h * tebalInti));

                sky += bintangCol * (1.0h - awanAlpha);
                sky = lerp(sky, awanCol, awanAlpha);

                half horizonMix = smoothstep((half)_HorizonHeight, -0.03h, (half)rawDir.y);
                half3 col = lerp(sky, _HorizonColor.rgb + half3(0.18h, 0.22h, 0.28h) * (half)_Lightning, horizonMix);
                return half4(max(col, 0.0h), 1.0h);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
