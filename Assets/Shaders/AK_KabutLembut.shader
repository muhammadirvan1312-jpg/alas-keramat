Shader "AlasKeramat/KabutLembut"
{
    Properties
    {
        _BaseColor ("Warna Kabut", Color) = (0.95, 0.92, 0.80, 0.14)
        _Mode ("Mode (0=KabutRadial, 1=BerkasCahaya)", Range(0,1)) = 0
        _Kecepatan ("Kecepatan Gerak", Range(0,1)) = 0.08
        _SkalaNoise ("Skala Noise", Range(0.5,8)) = 2.6
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+10" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Mode, _Kecepatan, _SkalaNoise;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise2(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(Hash21(i), Hash21(i + float2(1, 0)), f.x),
                    lerp(Hash21(i + float2(0, 1)), Hash21(i + float2(1, 1)), f.x),
                    f.y);
            }

            float Fbm2(float2 p)
            {
                return Noise2(p) * 0.62 + Noise2(p * 2.13 + 7.3) * 0.38;
            }

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = NormalizeNormalPerVertex(TransformObjectToWorldNormal(v.normalOS));
                o.uv = v.uv;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half3 viewDir = GetWorldSpaceNormalizeViewDir(i.positionWS);
                half3 nrm = normalize(i.normalWS);

                // Mode 0: Kabut lembah melingkar halus tanpa tepi kotak sama sekali.
                float dist = length(i.uv - float2(0.5, 0.5)) * 2.0;
                half radial = pow(saturate(1.0h - (half)dist), 2.35h);
                float2 uvMist = i.positionWS.xz * (0.045 * _SkalaNoise) + float2(_Time.y * _Kecepatan, _Time.y * _Kecepatan * 0.55);
                half wisp = smoothstep(0.24h, 0.84h, (half)Fbm2(uvMist));
                half alphaKabut = radial * (0.35h + 0.65h * wisp);

                // Mode 1: Berkas cahaya matahari (God-Ray) dengan tepi Fresnel & pudar vertikal halus + pudar otomatis saat malam/badai.
                half fresnelSoft = pow(saturate(abs(dot(nrm, viewDir))), 2.5h);
                half vFade = smoothstep(0.0h, 0.28h, (half)i.uv.y) * smoothstep(1.0h, 0.18h, (half)i.uv.y);
                half streak = 0.55h + 0.45h * (half)Noise2(float2(i.uv.x * 7.5, i.uv.y * 1.4 - _Time.y * 0.08));
                Light sun = GetMainLight();
                half sunElev = smoothstep(0.08h, 0.32h, (half)sun.direction.y);
                half sunBright = saturate(dot(sun.color, half3(0.3h, 0.59h, 0.11h)) * 0.85h);
                half alphaRay = fresnelSoft * vFade * streak * sunElev * sunBright;

                half alpha = lerp(alphaKabut, alphaRay, (half)_Mode) * _BaseColor.a;
                // Pudarkan halus ketika kamera terlalu dekat agar tidak memotong wajah kamera.
                half camDist = length(_WorldSpaceCameraPos - i.positionWS);
                alpha *= smoothstep(2.0h, 9.0h, camDist);

                half3 col = lerp(_BaseColor.rgb, _BaseColor.rgb * max(sun.color, half3(0.45h, 0.40h, 0.30h)), (half)_Mode * 0.7h);
                col = MixFog(col, i.fogFactor);
                return half4(col, saturate(alpha));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
