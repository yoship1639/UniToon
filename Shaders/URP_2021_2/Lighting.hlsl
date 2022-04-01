#ifndef UNITOON_2021_2_LIGHTING_INCLUDED
#define UNITOON_2021_2_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#if !defined(INV_PI)
#define INV_PI 0.318309
#endif

void UniToonLightingPhysicallyBased(BRDFData brdfData,
    half3 lightColor, half3 lightDirectionWS, half distanceAttenuation, half shadowAttenuation,
    half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff,
    half3 shadeColor, half toonyFactor, out half3 color, out half ramp, out half3 spec)
{
    half NdotL = saturate(dot(normalWS, lightDirectionWS));
    half lightAttenuation = distanceAttenuation * shadowAttenuation;
    ramp = 1.0 - NdotL;
    ramp = pow(ramp, 1 / toonyFactor);
    ramp = 1.0 - ramp;
    ramp *= shadowAttenuation;
    color = lightColor * lightAttenuation * brdfData.diffuse;
    spec = 0;
    half3 col = lightColor * distanceAttenuation * lerp(shadeColor, brdfData.diffuse, ramp);
    half3 radiance = lightColor * (lightAttenuation * NdotL);

#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        spec = brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS) * radiance;
    }
#endif // _SPECULARHIGHLIGHTS_OFF
}

void UniToonLightingPhysicallyBased(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff, half3 shadeColor, half toonyFactor, out half3 color, out half ramp, out half3 spec)
{
    UniToonLightingPhysicallyBased(brdfData, light.color, light.direction, light.distanceAttenuation, light.shadowAttenuation, normalWS, viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec);
}

half3 UniToonLightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    half NdotL = saturate(dot(normal, lightDir));
    return lightColor * NdotL;
}

half3 UniToonCalculateBlinnPhong(Light light, InputData inputData, SurfaceData surfaceData)
{
    half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
    half3 lightColor = UniToonLightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);

    lightColor *= surfaceData.albedo;

    #if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    half smoothness = exp2(10 * surfaceData.smoothness + 1);

    lightColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, half4(surfaceData.specular, 1), smoothness);
    #endif

    return lightColor;
}


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

    half3 totalColor = 0;
    totalRamp = 0;
    half3 totalSpec = 0;

    half3 color = 0;
    half ramp = 0;
    half3 spec = 0;

    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        UniToonLightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec);
        totalColor += color;
        totalRamp += ramp;
        totalSpec += spec;
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
            UniToonLightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec);
            totalColor += color;
            totalRamp += ramp;
            totalSpec += spec;
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        half lightColorMax = max(light.color.r, max(light.color.g, light.color.b));
        if (lightColorMax > _AdditionalLightHiCut) light.color = (light.color / max(lightColorMax, 1.0)) * _AdditionalLightHiCut;

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            UniToonLightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS, specularHighlightsOff, shadeColor, toonyFactor, color, ramp, spec);
            totalColor += color;
            totalRamp += ramp;
            totalSpec += spec;
        }
    LIGHT_LOOP_END
    #endif

    totalRamp = saturate(totalRamp);
    lightingData.mainLightColor = lerp(shadeColor, max(shadeColor, totalColor), totalRamp) * INV_PI * _PostDiffuseIntensity + totalSpec * _PostSpecularIntensity;
    //lightingData.mainLightColor *= UNITOON_BRIGHTNESS;
    lightingData.additionalLightsColor = 0;

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}

// TODO: UniToon SimpleLit
half4 UniToonFragmentBlinnPhong(InputData inputData, SurfaceData surfaceData, half3 shadeColor, half toonyFactor)
{
    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
    {
        return debugColor;
    }
    #endif

    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);

    inputData.bakedGI *= surfaceData.albedo;

    LightingData lightingData = CreateLightingData(inputData, surfaceData);
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        lightingData.mainLightColor += UniToonCalculateBlinnPhong(mainLight, inputData, surfaceData);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += UniToonCalculateBlinnPhong(light, inputData, surfaceData);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += UniToonCalculateBlinnPhong(light, inputData, surfaceData);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}

#endif
