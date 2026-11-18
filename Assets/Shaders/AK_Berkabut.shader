Shader "AlasKeramat/KabutLokal"
{
    Properties
    {
        _BaseMap("Noise lembut",2D)="white"{}
        _BaseColor("Warna",Color)=(0.5,0.6,0.65,0.05)
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Name "UniversalForward"
            Tags {"LightMode"="UniversalForward"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;half4 _BaseColor;
            CBUFFER_END
            struct AKFogVertex {float4 p:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct AKFogPixel {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float4 screen:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID};
            AKFogPixel Vert(AKFogVertex v)
            {
                UNITY_SETUP_INSTANCE_ID(v);AKFogPixel o=(AKFogPixel)0;UNITY_TRANSFER_INSTANCE_ID(v,o);
                o.world=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.world);o.screen=ComputeScreenPos(o.p);o.uv=v.uv;return o;
            }
            half4 Frag(AKFogPixel i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float2 uv=i.uv;
                half noise=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv*2+_Time.y*float2(0.006,0.002)).r;
                half edge=saturate(uv.x*(1-uv.x)*6)*saturate(uv.y*(1-uv.y)*6);
                float scene=LinearEyeDepth(SampleSceneDepth(i.screen.xy/i.screen.w),_ZBufferParams);
                float own=-TransformWorldToView(i.world).z;
                half soft=saturate((scene-own)*0.4);
                half dist=saturate((distance(_WorldSpaceCameraPos,i.world)-5)/15);
                return half4(_BaseColor.rgb,_BaseColor.a*noise*edge*soft*dist);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
