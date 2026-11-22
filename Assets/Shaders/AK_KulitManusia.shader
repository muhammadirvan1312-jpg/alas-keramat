Shader "AlasKeramat/KulitManusia"
{
    Properties
    {
        [MainTexture] _BaseMap ("Tekstur dasar", 2D) = "white" {}
        [MainColor] _BaseColor ("Warna dasar", Color) = (1,1,1,1)
        _Smoothness ("Kehalusan", Range(0,1)) = 0.3
        _Goyang ("Goyang batang", Range(0,0.1)) = 0
        [Normal] _BumpMap ("Normal kulit", 2D) = "bump" {}
        _BumpScale ("Kekuatan normal", Range(0,2)) = 1
        _Metallic ("Metalik", Range(0,1)) = 0
        _MaskMap ("AO dan roughness",2D)="white"{}
        _PakaiMask ("Mask aktif",Range(0,1))=0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap);
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _Smoothness, _Goyang;
            float _BumpScale, _Metallic, _PakaiMask;
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

            o.positionWS = p;
            o.positionCS = TransformWorldToHClip(p);
            o.normalWS = n;
            o.tangentWS = half4(SafeNormalize(t), v.tangentOS.w * GetOddNegativeScale());
            o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
            o.color = v.color;

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
            
            return c;
        }
        half3 Normal(Varyings i, FRONT_FACE_TYPE face)
        {
            half3 n = NormalizeNormalPerPixel(i.normalWS);
            half3 t = SafeNormalize(i.tangentWS.xyz - n * dot(n, i.tangentWS.xyz));
            half3 b = cross(n, t) * i.tangentWS.w;
            half3 nt = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv), _BumpScale);
            return NormalizeNormalPerPixel(TransformTangentToWorld(nt, half3x3(t, b, n)));
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
            s.albedo = c.rgb;
            s.alpha = 1;
            s.metallic = _Metallic;
            half4 mask=SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,i.uv);
            half wet=_AK_Basah*saturate(d.normalWS.y*0.7+0.4);
            s.albedo*=1-wet*0.2;
            s.smoothness = saturate(lerp(_Smoothness,mask.a,_PakaiMask)+wet*0.35);
            s.occlusion = lerp(1,mask.g,_PakaiMask);
            s.normalTS = half3(0,0,1);
            half4 lit = UniversalFragmentPBR(d, s);

            Light skinLight=GetMainLight(d.shadowCoord,d.positionWS,half4(1,1,1,1));
            half ndv=saturate(dot(d.normalWS,d.viewDirectionWS));
            half ndl=dot(d.normalWS,skinLight.direction);
            half wrap=saturate((ndl+0.62h)/1.62h);
            // Subsurface scattering hangat di tepi bayangan (terminator) & cuping/pipi agar kulit tidak pucat abu-abu.
            half termBand=saturate(1.0h-abs(ndl)*2.2h)*0.65h+wrap*0.55h;
            half fresnelSheen=pow(1.0h-ndv,3.2h)*0.11h;
            half3 warmSss=half3(0.46h,0.165h,0.092h);
            half shadowMix=0.38h+0.62h*skinLight.shadowAttenuation;
            lit.rgb+=c.rgb*warmSss*termBand*0.56h*skinLight.color*shadowMix*skinLight.distanceAttenuation;
            // Pantulan halus pori/minyak alami kulit (dual-lobe micro specular) + pantulan hangat tanah/langit.
            half3 halfVec=SafeNormalize(skinLight.direction+d.viewDirectionWS);
            half ndh=saturate(dot(d.normalWS,halfVec));
            half skinSpec=pow(ndh,26.0h)*0.085h*(0.35h+0.65h*s.smoothness)*shadowMix*skinLight.distanceAttenuation;
            lit.rgb+=skinLight.color*skinSpec;
            half hemiWarm=saturate(-d.normalWS.y*0.5h+0.5h);
            half3 bounceFill=lerp(half3(0.11h,0.12h,0.15h),half3(0.16h,0.12h,0.085h),hemiWarm);
            lit.rgb+=c.rgb*bounceFill*0.24h;
            lit.rgb+=fresnelSheen*(c.rgb*0.52h+skinLight.color*0.36h)*(0.42h+0.58h*shadowMix);
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
