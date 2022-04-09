#ifndef UNITOON_2021_2_LIGHTING_INCLUDED
#define UNITOON_2021_2_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../UniToonFunctions.hlsl"
#include "../UniToonLighting.hlsl"

half4 UniToonFragmentPBR(InputData inputData, SurfaceData surfaceData, half3 shadeColor, half toonyFactor, out half totalRamp)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLightLayer();

    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    half mainLightColorMax = max(mainLight.color.r, max(mainLight.color.g, mainLight.color.b));
    if (mainLightColorMax > _MainLightHiCut) mainLight.color = (mainLight.color / max(mainLightColorMax, 1.0)) * _MainLightHiCut;

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS) * _PostGIIntensity;

    half3 totalBright = lightingData.giColor;
    half3 totalColor = 0;
    totalRamp = 0;
    half3 totalSpec = 0;

    half3 bright = 0;
    half3 color = 0;
    half ramp = 0;
    half3 spec = 0;

    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        UniToonLightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec, bright);
        totalColor += color;
        totalRamp += ramp;
        totalSpec += spec;
        totalBright += bright;
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        half lightColorMax = max(light.color.r, max(light.color.g, light.color.b));
        if (lightColorMax > _AdditionalLightHiCut) light.color = (light.color / max(lightColorMax, 1.0)) * _AdditionalLightHiCut;

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            UniToonLightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec, bright);
            totalColor += color;
            totalRamp += ramp;
            totalSpec += spec;
            totalBright += bright;
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        half lightColorMax = max(light.color.r, max(light.color.g, light.color.b));
        if (lightColorMax > _AdditionalLightHiCut) light.color = (light.color / max(lightColorMax, 1.0)) * _AdditionalLightHiCut;

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            UniToonLightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec, bright);
            totalColor += color;
            totalRamp += ramp;
            totalSpec += spec;
            totalBright += bright;
        }
    LIGHT_LOOP_END
    #endif

    totalRamp = saturate(totalRamp);
    half finalBright = saturate(maxcolor(totalBright));
    finalBright = saturate(lerp(_PostMinBrightness, 1, finalBright * 4.0));
    lightingData.mainLightColor = lerp(shadeColor, max(shadeColor, totalColor), totalRamp) * INV_PI * _PostDiffuseIntensity + totalSpec * _PostSpecularIntensity;
    lightingData.mainLightColor *= finalBright;
    lightingData.additionalLightsColor = 0;

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}

#endif
