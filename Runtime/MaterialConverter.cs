using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniToon
{
    public enum UniToonVersion
    {
        Unknown = 0,
        URP_2021_3 = 1213,
        URP_2021_2 = 1212,
        URP_2021_1 = 1211,
        URP_2020_3 = 1203,
        URP_2020_2 = 1202,
        URP_2020_1 = 1201,
        URP_2019_4 = 1194,
        URP_2019_3 = 1193,
    }

    public enum WorkflowMode
	{
		Specular,
		Metallic,
	}
	
	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,
		Transparent,
        FadeWithZWrite,
		TransparentWithZWrite,
	}

	public enum SmoothnessMapChannel
	{
		SpecularMetallicAlpha,
		AlbedoAlpha
	}

    public enum RenderFace
    {
        Front = 2,
        Back = 1,
        Both = 0
    }

    public class MaterialConverter
    {
        enum RenderPipelineType
        {
            Builtin = 0,
            URP = 1,
            HDRP = 2,
        }

        public static UniToonVersion GetCurrentVersion()
        {
            if (GraphicsSettings.currentRenderPipeline?.GetType().Name == "UniversalRenderPipelineAsset")
            {
#if UNITY_2021_3
                return UniToonVersion.URP_2021_3;
#elif UNITY_2021_2
                return UniToonVersion.URP_2021_2;
#elif UNITY_2021_1
                return UniToonVersion.URP_2021_1;
#elif UNITY_2020_3
                return UniToonVersion.URP_2020_3;
#elif UNITY_2020_2
                return UniToonVersion.URP_2020_2;
#elif UNITY_2020_1
                return UniToonVersion.URP_2020_1;
#elif UNITY_2019_4
                return UniToonVersion.URP_2019_4;
#elif UNITY_2019_3
                return UniToonVersion.URP_2019_3;
#else
                return UniToonVersion.Unknown;
#endif
            }
            else if (GraphicsSettings.currentRenderPipeline?.GetType().Name == "HDRenderPipelineAsset")
            {
#if UNITY_2021_2
                return UniToonVersion.Unknown;
#elif UNITY_2021_1
                return UniToonVersion.Unknown;
#elif UNITY_2020_3
                return UniToonVersion.Unknown;
#elif UNITY_2020_2
                return UniToonVersion.Unknown;
#elif UNITY_2020_1
                return UniToonVersion.Unknown;
#elif UNITY_2019_4
                return UniToonVersion.Unknown;
#elif UNITY_2019_3
                return UniToonVersion.Unknown;
#else
                return UniToonVersion.Unknown;
#endif
            }
            else
            {
#if UNITY_2021_2
                return UniToonVersion.Unknown;
#elif UNITY_2021_1
                return UniToonVersion.Unknown;
#elif UNITY_2020_3
                return UniToonVersion.Unknown;
#elif UNITY_2020_2
                return UniToonVersion.Unknown;
#elif UNITY_2020_1
                return UniToonVersion.Unknown;
#elif UNITY_2019_4
                return UniToonVersion.Unknown;
#elif UNITY_2019_3
                return UniToonVersion.Unknown;
#else
                return UniToonVersion.Unknown;
#endif
            }
        }

        private static Texture GetTexture(Material mat, string propertyName)
        {
            return mat.HasProperty(propertyName) ? mat.GetTexture(propertyName) : null;
        }

        public static void MaterialChanged(Material mat, UniToonVersion version, bool updateRenderQueue = false)
        {
            var rp = (RenderPipelineType)((int)version / 1000);

            // clear keywords
            mat.shaderKeywords = new string[0];

            // normal map
            SetKeyword(mat, "_NORMALMAP", GetTexture(mat, "_BumpMap") || GetTexture(mat, "_DetailNormalMap"));

            // workflow mode
            if ((WorkflowMode)mat.GetFloat("_WorkflowMode") == WorkflowMode.Specular)
            {
                if (rp == RenderPipelineType.Builtin)
                {
                    SetKeyword(mat, "_METALLICGLOSSMAP", GetTexture(mat, "_SpecGlossMap"));
                }
                else if (rp == RenderPipelineType.URP)
                {
                    SetKeyword(mat, "_METALLICSPECGLOSSMAP", GetTexture(mat, "_SpecGlossMap"));
                    SetKeyword(mat, "_SPECULAR_SETUP", true);
                }
            }
            else
            {
                if (rp == RenderPipelineType.Builtin)
                {
                    SetKeyword(mat, "_METALLICGLOSSMAP", GetTexture(mat, "_MetallicGlossMap"));
                }
                else if (rp == RenderPipelineType.URP)
                {
                    SetKeyword(mat, "_METALLICSPECGLOSSMAP", GetTexture(mat, "_MetallicGlossMap"));
                }
            }
            
            // parallax map
            SetKeyword(mat, "_PARALLAXMAP", GetTexture(mat, "_ParallaxMap"));

            // occlusion map
            if (rp == RenderPipelineType.URP)
            {
                SetKeyword(mat, "_OCCLUSIONMAP", GetTexture(mat, "_OcclusionMap"));
            }

            // detail map
            SetKeyword(mat, "_DETAIL_MULX2", GetTexture(mat, "_DetailAlbedoMap") || GetTexture(mat, "_DetailNormalMap"));

            // emission
            SetEmissionKeyword(mat);

            // smoothness texture channel
            SetKeyword(mat, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", (SmoothnessMapChannel)mat.GetFloat("_SmoothnessTextureChannel") == SmoothnessMapChannel.AlbedoAlpha);

            // specular highlights
            SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", mat.GetFloat("_SpecularHighlights") < 0.5f);

            // environment reflections
            if (rp == RenderPipelineType.Builtin)
            {
                SetKeyword(mat, "_GLOSSYREFLECTIONS_OFF", mat.GetFloat("_EnvironmentReflections") < 0.5f);
            }
            else if (rp == RenderPipelineType.URP)
            {
                SetKeyword(mat, "_ENVIRONMENTREFLECTIONS_OFF", mat.GetFloat("_EnvironmentReflections") < 0.5f);
            }

            // receive shadow
            if (rp == RenderPipelineType.URP)
            {
                SetKeyword(mat, "_RECEIVE_SHADOWS_OFF", mat.GetFloat("_ReceiveShadow") < 0.5f);
            }

            // blend mode
            SetBlendMode(mat, updateRenderQueue);
        }

        private static void SetEmissionKeyword(Material mat)
        {
            var color = mat.GetColor("_EmissionColor");
            var realtimeEmission = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.RealtimeEmissive) > 0;
            SetKeyword(mat, "_EMISSION", color.maxColorComponent > 0.1f / 255.0f || realtimeEmission);
        }

        private static void SetBlendMode(Material mat, bool updateRenderQueue = false)
        {
            var blendMode = (BlendMode)mat.GetFloat("_Blend");

            switch (blendMode)
            {
                case BlendMode.Opaque:
                    mat.SetOverrideTag("RenderType", "");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    if (updateRenderQueue) mat.renderQueue = -1;
                    break;

                case BlendMode.Cutout:
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    if (updateRenderQueue) mat.renderQueue = (int)RenderQueue.AlphaTest;
                    break;

                case BlendMode.Fade:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    if (updateRenderQueue) mat.renderQueue = (int)RenderQueue.Transparent;
                    break;

                case BlendMode.Transparent:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    if (updateRenderQueue) mat.renderQueue = (int)RenderQueue.Transparent;
                    break;

                case BlendMode.FadeWithZWrite:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 1);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    if (updateRenderQueue) mat.renderQueue = (int)RenderQueue.Transparent;
                    break;

                case BlendMode.TransparentWithZWrite:
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 1);
                    mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    if (updateRenderQueue) mat.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }

        private static void SetKeyword(Material mat, string keyword, bool flag)
        {
            if (flag)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }
    }
}
