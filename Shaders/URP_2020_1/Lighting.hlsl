#ifndef UNITOON_2020_1_LIGHTING_INCLUDED
#define UNITOON_2020_1_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../UniToonFunctions.hlsl"
#include "../UniToonLighting.hlsl"

half4 UniToonFragmentPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
    half smoothness, half occlusion, half3 emission, half alpha, 
    half3 shadeColor, half toonyFactor, out half totalRamp)
{
#ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
#endif

    BRDFData brdfData;
    InitializeBRDFData(albedo, metallic, specular, smoothness, alpha, brdfData);
    
    Light mainLight = GetMainLight(inputData.shadowCoord);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

    half3 gi = GlobalIllumination(brdfData, inputData.bakedGI, occlusion, inputData.normalWS, inputData.viewDirectionWS);

    half3 totalBright = gi;
    half3 totalColor = 0;
    totalRamp = 0;
    half3 totalSpec = 0;

    half3 bright = 0;
    half3 color = 0;
    half ramp = 0;
    half3 spec = 0;

    UniToonLightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec, bright);
    totalColor += color;
    totalRamp += ramp;
    totalSpec += spec;
    totalBright += bright;

#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        UniToonLightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec, bright);
        totalColor += color;
        totalRamp += ramp;
        totalSpec += spec;
        totalBright += bright;
    }
#endif

    totalRamp = saturate(totalRamp);
    half finalBright = saturate(maxcolor(totalBright));
    finalBright = saturate(lerp(_PostMinBrightness, 1, finalBright * 4.0));
    totalColor = lerp(shadeColor, max(shadeColor, totalColor), totalRamp) * INV_PI * _PostDiffuseIntensity + totalSpec * _PostSpecularIntensity;
    totalColor *= finalBright;
    totalColor += gi * _PostGIIntensity;

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    totalColor += inputData.vertexLighting * brdfData.diffuse;
#endif

    totalColor += emission;
    return half4(totalColor, alpha);
}

#endif
