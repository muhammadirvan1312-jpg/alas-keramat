Shader "AlasKeramat/Daun"
{
    Properties
    {
        [MainTexture] _BaseMap ("Tekstur dasar", 2D) = "white" {}
        [MainColor] _BaseColor ("Warna dasar", Color) = (1,1,1,1)
        [Normal] _BumpMap ("Normal daun", 2D) = "bump" {}
        _BumpScale ("Detail normal", Range(0,2)) = 0.65
        _Smoothness ("Kehalusan", Range(0,1)) = 0.25
        _Goyang ("Goyang batang", Range(0,0.1)) = 0.025
        _Cutoff ("Ambang alpha", Range(0,1)) = 0.45
        _Kibasan ("Kibasan daun (meter)", Range(0,0.25)) = 0.06
        _Translucency ("Cahaya tembus", Range(0,1)) = 0.45
        _VariasiWarna ("Variasi tiap pohon", Range(0,0.4)) = 0.16
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Cull Off
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _Smoothness, _Goyang, _BumpScale;
            float _Cutoff, _Kibasan, _Translucency, _VariasiWarna;
        CBUFFER_END
        // Global, sengaja bukan Properties/UnityPerMaterial.
        float4 _AK_AnginArah;
        float _AK_AnginKuat, _AK_AnginWaktu, _AK_Basah;
        float3 _LightDirection, _LightPosition;
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half4 tangentWS : TEXCOORD2;
            float2 uv : TEXCOORD3;
            half4 color : TEXCOORD4;
            half4 fogLight : TEXCOORD5;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        Varyings Vert(Attributes v)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            Varyings o = (Varyings)0;
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            float3 pivot = TransformObjectToWorld(float3(0,0,0));
            float3 p = TransformObjectToWorld(v.positionOS.xyz);
            float3 n = TransformObjectToWorldNormal(v.normalOS);
            float3 t = TransformObjectToWorldDir(v.tangentOS.xyz);
            float3 dir = float3(_AK_AnginArah.x, 0, _AK_AnginArah.z);
            float h = max(p.y - pivot.y, 0);
            // Resep batang identik pada Daun/Kulit, termasuk fase dan amplitudo.
            const float Lentur = 8.0;
            float phase = dot(pivot.xz, float2(0.071, 0.053));
            float sway = (sin(_AK_AnginWaktu * 1.1 + phase) + 0.35 * sin(_AK_AnginWaktu * 1.9 + phase));
            float amp = sway * _Goyang * _AK_AnginKuat;
            p += dir * (amp * h * h / (h + Lentur));
            float slope = amp * h * (h + 2 * Lentur) / ((h + Lentur) * (h + Lentur));
            n.y -= dot(n, dir) * slope;
            t += dir * (t.y * slope);
            n = SafeNormalize(n);
            // Noise fase per vertex; UV atlas dan warna AO tidak dipakai sebagai mask angin.
            float seed = dot(v.positionOS.xyz, float3(1.37, 2.91, 0.73));
            float flutter = sin(seed + _AK_AnginWaktu * 7.1) * sin(seed * 1.63 - _AK_AnginWaktu * 4.3);
            p += n * (flutter * _Kibasan * _AK_AnginKuat * h / (h + 0.3));
            o.positionWS = p;
            o.positionCS = TransformWorldToHClip(p);
            o.normalWS = n;
            o.tangentWS = half4(SafeNormalize(t), v.tangentOS.w * GetOddNegativeScale());
            o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
            o.color = v.color;
            float hash = frac(sin(dot(pivot.xz, float2(12.9898, 78.233))) * 43758.5453);
            o.color.rgb *= lerp(half3(1,1,1), half3(0.83 + hash * 0.34, 0.92 + hash * 0.16, 0.78 + hash * 0.30), _VariasiWarna / 0.4);
            o.fogLight = half4(ComputeFogFactor(o.positionCS.z), VertexLighting(p, n));
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
        half4 Base(Varyings i)
        {
            half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor * i.color;
            clip(c.a - _Cutoff);
            return c;
        }
        half3 Normal(Varyings i, FRONT_FACE_TYPE face)
        {
            half3 n=NormalizeNormalPerPixel(i.normalWS);
            half3 t=SafeNormalize(i.tangentWS.xyz-n*dot(n,i.tangentWS.xyz));
            half3 b=cross(n,t)*i.tangentWS.w;
            half3 detail=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
            return NormalizeNormalPerPixel(TransformTangentToWorld(detail,half3x3(t,b,n))) * IS_FRONT_VFACE(face,1.0,-1.0);
        }
        half4 Frag(Varyings i, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            half4 c = Base(i);
            InputData d = (InputData)0;
            d.positionWS = i.positionWS;
            d.normalWS = Normal(i, face);
            d.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
            d.shadowCoord = ComputeScreenPos(TransformWorldToHClip(i.positionWS));
            #else
            d.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
            #endif
            d.bakedGI = SampleSH(d.normalWS);
            d.vertexLighting = i.fogLight.yzw;
            d.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
            d.shadowMask = half4(1,1,1,1);
            SurfaceData s = (SurfaceData)0;
            half ao = saturate(0.55h + 0.45h * i.color.g) * saturate(0.75h + 0.25h * d.normalWS.y);
            half wet = _AK_Basah * saturate(d.normalWS.y * 0.7h + 0.4h);
            s.albedo = c.rgb * (0.86h + 0.14h * ao) * (1.0h - wet * 0.14h);
            s.alpha = 1;
            s.metallic = 0;
            s.smoothness = saturate(_Smoothness * 0.42h + wet * 0.38h);
            s.occlusion = ao;
            s.normalTS = half3(0,0,1);
            half4 lit = UniversalFragmentPBR(d, s);
            Light sun = GetMainLight(d.shadowCoord, d.positionWS, half4(1,1,1,1));
            half back = saturate(dot(-d.normalWS, sun.direction) * 0.75h + 0.25h);
            half towardSun = pow(saturate(dot(-d.viewDirectionWS, sun.direction)), 3);
            half rim = pow(1.0h - saturate(dot(d.normalWS, d.viewDirectionWS)), 3) * 0.12h * back;
            half skySheen = pow(1.0h - saturate(dot(d.normalWS, d.viewDirectionWS)), 3.0h) * wet * 0.22h;
            half3 sssTint = s.albedo * half3(1.05h, 1.34h, 0.56h);
            lit.rgb += sssTint * sun.color * ((_Translucency * 1.42h * back * (0.34h + 0.66h * towardSun) + rim * 1.35h)
                * sun.distanceAttenuation * sun.shadowAttenuation)
                + sun.color * (skySheen * sun.distanceAttenuation);
            lit.rgb = MixFog(lit.rgb, i.fogLight.x);
            return lit;
        }
        half4 DepthFrag(Varyings i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            Base(i);
            return 0;
        }
        half4 ShadowFrag(Varyings i) : SV_Target { return DepthFrag(i); }
        half4 NormalsFrag(Varyings i, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            Base(i);
            float3 n = Normal(i, face);
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
            ZWrite On
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
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
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
