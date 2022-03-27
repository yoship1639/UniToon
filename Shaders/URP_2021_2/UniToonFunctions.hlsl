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

half3 shift(half3 color, half3 shift)
{
    half VSU = shift.z * shift.y * cos(shift.x * 6.28318512);
    half VSW = shift.z * shift.y * sin(shift.x * 6.28318512);
        
    return half3(
        (0.299 * shift.z + 0.701 * VSU + 0.168 * VSW) * color.r + (0.587 * shift.z - 0.587 * VSU + 0.330 * VSW) * color.g + (0.114 * shift.z - 0.114 * VSU - 0.497 * VSW) * color.b,
        (0.299 * shift.z - 0.299 * VSU - 0.328 * VSW) * color.r + (0.587 * shift.z + 0.413 * VSU + 0.035 * VSW) * color.g + (0.114 * shift.z - 0.114 * VSU + 0.292 * VSW) * color.b,
        (0.299 * shift.z - 0.300 * VSU + 1.25 * VSW)  * color.r + (0.587 * shift.z - 0.588 * VSU - 1.05 * VSW)  * color.g + (0.114 * shift.z + 0.886 * VSU - .203 * VSW) * color.b
    );
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
    float str = pow(10.0, strength * 10.0);
    float smoothness = 1.0 / remap(power, 0.0, 1.0, 0.01, 0.3);
    [unroll]
    for (int j = 0; j < 8; j++)
    {
        float sub = abs(depthes[j] - sceneDepth);
        //sub = saturate(sub * 100);
        sub = pow(sub, smoothness);
        //sub *= 0.01;
        sub *= str * coeff[j];
        //sub = pow(sub, power) * str * coeff[j];
        depthValue += sub;
    }

    half outlineRate = saturate(depthValue);

    return outlineRate;
}

#endif
