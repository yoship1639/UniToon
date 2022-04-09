#ifndef UNITOON_2021_1_LIGHTING_INCLUDED
#define UNITOON_2021_1_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../UniToonFunctions.hlsl"
#include "../UniToonLighting.hlsl"

half4 UniToonFragmentPBR(InputData inputData, SurfaceData surfaceData, half3 shadeColor, half toonyFactor, out half totalRamp)
{
#ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
#endif

    BRDFData brdfData;

    // NOTE: can modify alpha
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    BRDFData brdfDataClearCoat = (BRDFData)0;
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    // base brdfData is modified here, rely on the compiler to eliminate dead computation by InitializeBRDFData()
    InitializeBRDFDataClearCoat(surfaceData.clearCoatMask, surfaceData.clearCoatSmoothness, brdfData, brdfDataClearCoat);
#endif

    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
#elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    half mainLightColorMax = max(mainLight.color.r, max(mainLight.color.g, mainLight.color.b));
    if (mainLightColorMax > _MainLightHiCut) mainLight.color = (mainLight.color / max(mainLightColorMax, 1.0)) * _MainLightHiCut;

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion);
    #endif

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
    half3 gi = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                     inputData.bakedGI, surfaceData.occlusion,
                                     inputData.normalWS, inputData.viewDirectionWS);
    half3 totalBright = gi;
    half3 totalColor = 0;
    totalRamp = 0;
    half3 totalSpec = 0;

    half3 bright = 0;
    half3 color = 0;
    half ramp = 0;
    half3 spec = 0;

    UniToonLightingPhysicallyBased(brdfData, mainLight,
                                     inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff,
                                     shadeColor, toonyFactor,
                                     color, ramp, spec, bright);

    totalColor += color;
    totalRamp += ramp;
    totalSpec += spec;
    totalBright += bright;

#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        half lightColorMax = max(light.color.r, max(light.color.g, light.color.b));
        if (lightColorMax > _AdditionalLightHiCut) light.color = (light.color / max(lightColorMax, 1.0)) * _AdditionalLightHiCut;

        #if defined(_SCREEN_SPACE_OCCLUSION)
            light.color *= aoFactor.directAmbientOcclusion;
        #endif
        UniToonLightingPhysicallyBased(brdfData, light,
                                         inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff,
                                         shadeColor, toonyFactor,
                                         color, ramp, spec, bright);

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

    totalColor += surfaceData.emission;

    return half4(totalColor, surfaceData.alpha);
}

#endif
