#ifndef UNITOON_FUNCTIONS_INCLUDED
#define UNITOON_FUNCTIONS_INCLUDED

inline half invlerp(const half start, const half end, const half t)
{
    return (t - start) / (end - start);
}

inline half remap(const half v, const half fromMin, const half fromMax, const half toMin, const half toMax)
{
    return toMin + (v - fromMin) * (toMax - toMin) / (fromMax - fromMin);
}

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float sampleSceneDepth(float2 uv)
{
    float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
    return Linear01Depth(sceneDepth, _ZBufferParams) * _ProjectionParams.z;
}

float SoftOutline(float2 uv, half width, half strength, half power)
{
    float sceneDepth = sampleSceneDepth(uv);
    float w = width / max(sceneDepth * 1.0, 1.0);
    width = (w + 0.5) * 0.5;
    float2 delta = (1.0 / _ScreenParams.xy) * width;

    float depthes[8];
    depthes[0] = sampleSceneDepth(uv + float2(-delta.x, -delta.y));
    depthes[1] = sampleSceneDepth(uv + float2(-delta.x,  0.0)    );
    depthes[2] = sampleSceneDepth(uv + float2(-delta.x,  delta.y));
    depthes[3] = sampleSceneDepth(uv + float2(0.0,      -delta.y));
    depthes[4] = sampleSceneDepth(uv + float2(0.0,       delta.y));
    depthes[5] = sampleSceneDepth(uv + float2(delta.x,  -delta.y));
    depthes[6] = sampleSceneDepth(uv + float2(delta.x,   0.0)    );
    depthes[7] = sampleSceneDepth(uv + float2(delta.x,   delta.y));

    float coeff[8] = {0.5, 1.0, 0.5, 1.0, 1.0, 0.5, 1.0, 0.5};

    float depthValue = 0;
    strength = strength * 1000.0;
    [unroll]
    for (int j = 0; j < 8; j++)
    {
        float sub = abs(depthes[j] - sceneDepth);
        sub = pow(sub, power) * strength * coeff[j];
        depthValue += sub;
    }

    half outlineRate = saturate(depthValue);

    return outlineRate;
}

#endif
