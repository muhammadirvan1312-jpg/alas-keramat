Shader "AlasKeramat/SinarTajuk"
{
    Properties
    {
        _BaseMap("Peta berkas",2D)="white"{}
        _BaseColor("Warna",Color)=(1,0.92,0.72,0.34)
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Name "UniversalForward"
            Tags {"LightMode"="UniversalForward"}
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            float _AK_MatikanDepth;
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;half4 _BaseColor;
            CBUFFER_END
            struct AKSinarVertex {float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct AKSinarPixel {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float4 screen:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID};
            AKSinarPixel Vert(AKSinarVertex v)
            {
                UNITY_SETUP_INSTANCE_ID(v);AKSinarPixel o=(AKSinarPixel)0;UNITY_TRANSFER_INSTANCE_ID(v,o);
                o.world=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.world);o.screen=ComputeScreenPos(o.p);o.uv=v.uv;return o;
            }
            half4 Frag(AKSinarPixel i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float2 uv=i.uv;
                half shaft=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).r;
                half shimmer=0.78h+0.22h*SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(uv.x*1.7+_Time.y*0.012,uv.y*0.65-_Time.y*0.018)).r;
                half edge=saturate(uv.x*(1.0h-uv.x)*4.5h)*saturate(uv.y*(1.0h-uv.y)*3.5h);
                half soft=1.0h;
                if(_AK_MatikanDepth<0.5)
                {
                    float scene=LinearEyeDepth(SampleSceneDepth(i.screen.xy/i.screen.w),_ZBufferParams);
                    float own=-TransformWorldToView(i.world).z;
                    soft=saturate((scene-own)*0.35);
                }
                float camD=distance(_WorldSpaceCameraPos,i.world);
                half dist=saturate((camD-1.8)/6.0)*saturate(1.0-(camD-34.0)/18.0);
                return half4(_BaseColor.rgb,_BaseColor.a*shaft*shimmer*edge*soft*dist);
            }
            ENDHLSL
        }
        // Berkas sinar tidak menulis depth atau bayangan padat.
        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode"="ShadowCaster"}
            ZWrite Off ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex VertNoDepth
            #pragma fragment FragNoDepth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 VertNoDepth(float4 p:POSITION):SV_POSITION { return TransformObjectToHClip(p.xyz); }
            half4 FragNoDepth():SV_Target { clip(-1);return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags {"LightMode"="DepthOnly"}
            ZWrite Off ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex VertNoDepth
            #pragma fragment FragNoDepth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 VertNoDepth(float4 p:POSITION):SV_POSITION { return TransformObjectToHClip(p.xyz); }
            half4 FragNoDepth():SV_Target { clip(-1);return 0; }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
