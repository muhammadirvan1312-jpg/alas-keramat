Shader "AlasKeramat/Air"
{
    Properties
    {
        _WarnaDangkal ("Warna dangkal", Color) = (0.12,0.38,0.30,0.24)
        _WarnaDalam ("Warna dalam", Color) = (0.015,0.075,0.095,0.88)
        _JarakDalam ("Jarak serapan (meter)", Range(0.1,20)) = 4
        _SkalaNormal ("Frekuensi riak", Range(0.05,5)) = 0.8
        _KuatNormal ("Kekuatan riak", Range(0,1)) = 0.18
        _Kecepatan ("Kecepatan aliran", Range(0,3)) = 0.3
        _ArahAlir ("Arah aliran XY ke dunia XZ", Vector) = (1,0,0,0)
        _Smoothness ("Kehalusan", Range(0,1)) = 0.94
        _WarnaBuih ("Warna buih", Color) = (0.82,0.9,0.85,1)
        _LebarBuih ("Lebar buih (meter)", Range(0.01,2)) = 0.35
        _KuatBuih ("Kekuatan buih", Range(0,1)) = 0.75
        [ToggleUI] _GunakanDepth ("Depth kamera tersedia", Float) = 1
        [ToggleUI] _TulisDepth ("Ikut prepass depth khusus", Float) = 0
        [ToggleUI] _Bayangan ("Lempar bayangan padat", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Back
        HLSLINCLUDE
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _SPECULAR_SETUP 1
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _WarnaDangkal, _WarnaDalam, _WarnaBuih;
            float4 _ArahAlir;
            float _JarakDalam, _SkalaNormal, _KuatNormal, _Kecepatan;
            float _Smoothness, _LebarBuih, _KuatBuih;
            float _GunakanDepth, _TulisDepth, _Bayangan;
        CBUFFER_END
        float _AK_MatikanDepth;
        float _AK_Basah;
        float3 _LightDirection, _LightPosition;
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half4 fogLight : TEXCOORD2;
            float2 uv : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        Varyings Vert(Attributes v)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            Varyings o = (Varyings)0;
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
            o.positionCS = TransformWorldToHClip(o.positionWS);
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.uv = v.uv;
            o.fogLight = half4(ComputeFogFactor(o.positionCS.z), VertexLighting(o.positionWS, o.normalWS));
            return o;
        }
        Varyings ShadowVert(Attributes v)
        {
            Varyings o = Vert(v);
            #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
            float3 lightDir = SafeNormalize(_LightPosition - o.positionWS);
            #else
            float3 lightDir = _LightDirection;
            #endif
            o.positionCS = TransformWorldToHClip(ApplyShadowBias(o.positionWS, o.normalWS, lightDir));
            #if UNITY_REVERSED_Z
            o.positionCS.z = min(o.positionCS.z, UNITY_NEAR_CLIP_VALUE * o.positionCS.w);
            #else
            o.positionCS.z = max(o.positionCS.z, UNITY_NEAR_CLIP_VALUE * o.positionCS.w);
            #endif
            return o;
        }
        float Hash(float2 p)
        {
            float3 q = frac(float3(p.xyx) * 0.1031);
            q += dot(q, q.yzx + 33.33);
            return frac((q.x + q.y) * q.z);
        }
        // Value noise dan turunan analitik: 4 hash per lapis, tanpa tekstur normal.
        float3 NoiseGrad(float2 p)
        {
            float2 cell = floor(p), f = frac(p);
            float2 u = f * f * (3 - 2 * f), du = 6 * f * (1 - f);
            float a = Hash(cell), b = Hash(cell + float2(1,0));
            float c = Hash(cell + float2(0,1)), d = Hash(cell + 1);
            float k = a - b - c + d;
            return float3(a + (b-a)*u.x + (c-a)*u.y + k*u.x*u.y,
                du * (float2(b-a,c-a) + k*u.yx));
        }
        half3 WaterNormal(Varyings i, out float noise)
        {
            float2 flow = _ArahAlir.xy * (_Time.y * _Kecepatan);
            float2 p = i.positionWS.xz * _SkalaNormal;
            float3 a = NoiseGrad(p - flow);
            const float FrekuensiKedua = 1.93, BobotKedua = 0.5;
            float3 b = NoiseGrad(p * FrekuensiKedua + float2(17.3,9.2) - flow * 0.73);
            const float FrekuensiKetiga = 4.17, BobotKetiga = 0.35;
            float3 c = NoiseGrad(p * FrekuensiKetiga - float2(11.4,23.1) + flow * 1.28);
            float2 slope = (a.yz + b.yz * BobotKedua + c.yz * BobotKetiga) * (_KuatNormal * 1.35);
            // Riak lingkaran tetesan hujan dinamis di permukaan sungai & laut saat cuaca basah (_AK_Basah > 0).
            if (_AK_Basah > 0.05)
            {
                float2 rGrid = i.positionWS.xz * 2.1;
                float2 rCell = floor(rGrid), rFrac = frac(rGrid) - 0.5;
                float rSeed = Hash(rCell);
                float rAge = frac(_Time.y * 1.45 + rSeed * 6.3);
                float rDist = length(rFrac);
                float rWave = sin((rDist - rAge * 0.48) * 34.0) * (1.0 - rAge) * smoothstep(0.48, 0.05, rDist);
                slope += (rFrac / max(rDist, 0.02)) * (rWave * _AK_Basah * 0.24);
            }
            noise = (a.x + b.x * 0.75 + c.x * 0.45) / 2.2;
            half3 n = NormalizeNormalPerPixel(i.normalWS);
            half3 ripple = half3(-slope.x, 0, -slope.y);
            return NormalizeNormalPerPixel(n + ripple - n * dot(ripple,n));
        }
        float Thickness(Varyings i, float2 uv)
        {
            half depthOn = _GunakanDepth * (1.0h - saturate(_AK_MatikanDepth));
            if (depthOn < 0.5h) return _JarakDalam;
            float raw = SampleSceneDepth(uv);
            float sceneEye;
            if (unity_OrthoParams.w > 0.5)
            {
                #if UNITY_REVERSED_Z
                raw = 1 - raw;
                #endif
                sceneEye = lerp(_ProjectionParams.y, _ProjectionParams.z, raw);
            }
            else sceneEye = LinearEyeDepth(raw, _ZBufferParams);
            float waterEye = -TransformWorldToView(i.positionWS).z;
            return max(0, sceneEye - waterEye);
        }
        half4 Frag(Varyings i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            InputData inputData = (InputData)0;
            inputData.positionWS = i.positionWS;
            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
            float noise;
            half3 n = WaterNormal(i, noise);
            half3 view = GetWorldSpaceNormalizeViewDir(i.positionWS);
            float depth = Thickness(i, inputData.normalizedScreenSpaceUV);
            half absorption = 1 - exp(-depth / max(_JarakDalam, 0.001));
            half4 water = lerp(_WarnaDangkal, _WarnaDalam, absorption);
            half edge = 1 - saturate(depth / max(_LebarBuih, 0.001));
            half ring = 0.72h + 0.28h * sin(depth * 14.0 - _Time.y * 3.2);
            half depthFoam = smoothstep(0.22, 0.68, noise + edge * 0.48) * edge * ring * _KuatBuih;
            depthFoam *= _GunakanDepth * (1.0h - saturate(_AK_MatikanDepth));
            half noDepthFoam = saturate(_AK_MatikanDepth) * smoothstep(0.54, 0.86, noise) * _KuatBuih * 0.48h;
            half tepiUV = 1.0h - saturate(min(i.uv.x, 1.0 - i.uv.x) / 0.14);
            half bankFoam = smoothstep(0.26, 0.72, noise + tepiUV * 0.52) * tepiUV * _KuatBuih * 0.85h;
            half crestFoam = smoothstep(0.62, 0.86, noise) * _KuatBuih * 0.55h;
            half foam = saturate(max(max(depthFoam, noDepthFoam), max(bankFoam, crestFoam)));
            half kaustik = pow(saturate(noise * 1.12h), 3.0h) * (1.0h - absorption) * (1.0h - foam) * 0.20h;
            half3 albedo = lerp(water.rgb + half3(kaustik, kaustik * 0.96h, kaustik * 0.82h), _WarnaBuih.rgb, foam);
            half opacity = saturate(lerp(water.a, _WarnaBuih.a, foam));
            // Fresnel air (IOR 1.333), refleksi dipertahankan pada sudut mendatar.
            const half F0 = 0.0204h;
            half fresnel = F0 + (1 - F0) * pow(1 - saturate(dot(n,view)), 5);
            SurfaceData s = (SurfaceData)0;
            s.albedo = albedo;
            s.specular = half3(F0,F0,F0);
            s.smoothness = lerp(_Smoothness, 0.35h, foam);
            s.alpha = 1;
            BRDFData brdf;
            InitializeBRDFData(s, brdf);
            brdf.diffuse *= opacity;
            // Transparan memakai shadow map langsung, bukan screen-space opaque shadows.
            float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
            Light sun = GetMainLight(shadowCoord, i.positionWS, half4(1,1,1,1));
            half3 color = LightingPhysicallyBased(brdf, sun, n, view);
            half glint = pow(saturate(dot(reflect(-sun.direction, n), view)), 96.0h) * (1.0h - foam);
            half kilauLebar = pow(saturate(dot(reflect(-sun.direction, n), view)), 22.0h) * (1.0h - foam) * 0.22h;
            color += sun.color * ((glint * 1.45h + kilauLebar) * sun.distanceAttenuation * sun.shadowAttenuation);
            #if defined(_ADDITIONAL_LIGHTS)
            #if USE_FORWARD_PLUS
            [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
            {
                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
                color += LightingPhysicallyBased(brdf, GetAdditionalLight(lightIndex, i.positionWS, half4(1,1,1,1)), n, view);
            }
            #endif
            uint count = GetAdditionalLightsCount();
            LIGHT_LOOP_BEGIN(count)
                color += LightingPhysicallyBased(brdf, GetAdditionalLight(lightIndex, i.positionWS, half4(1,1,1,1)), n, view);
            LIGHT_LOOP_END
            #endif
            color += (SampleSH(n) + i.fogLight.yzw) * brdf.diffuse;
            color += GlossyEnvironmentReflection(reflect(-view,n), i.positionWS,
                brdf.perceptualRoughness, 1, inputData.normalizedScreenSpaceUV) * fresnel;
            half alpha = opacity + (1 - opacity) * fresnel;
            // Premultiplied: fog hanya menutupi fraksi permukaan air.
            color = MixFog(color / max(alpha, 0.001h), i.fogLight.x) * alpha;
            return half4(color, alpha);
        }
        // Default tidak menutupi dasar sungai di depth texture atau bayangan.
        // Aktifkan hanya bila integrator sengaja merender prepass air terpisah.
        half4 DepthFrag(Varyings i) : SV_Target { clip(_TulisDepth - 0.5); return 0; }
        half4 ShadowFrag(Varyings i) : SV_Target { clip(_Bayangan - 0.5); return 0; }
        half4 NormalsFrag(Varyings i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            clip(_TulisDepth - 0.5);
            float noise;
            float3 n = WaterNormal(i, noise);
            #if defined(_GBUFFER_NORMALS_OCT)
            float2 oct = PackNormalOctQuadEncode(n) * 0.5 + 0.5;
            return half4(PackFloat2To888(saturate(oct)), 0);
            #else
            return half4(n, 0);
            #endif
        }
        ENDHLSL
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment NormalsFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
