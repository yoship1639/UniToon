using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniToon
{
    public enum UniToonVersion
    {
        Unknown = 0,
        URP_2021_2 = 1212,
        URP_2021_1 = 1211,
        URP_2020_3 = 1203,
        URP_2020_2 = 1202,
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
        public static UniToonVersion GetCurrentVersion()
        {
#if UNITY_2021_2
            return UniToonVersion.URP_2021_2;
#elif UNITY_2021_1
            return UniToonVersion.URP_2021_1;
#elif UNITY_2020_3
            return UniToonVersion.URP_2020_3;
#elif UNITY_2020_2
            return UniToonVersion.URP_2020_2;
#else
            return UniToonVersion.Unknown;
#endif
        }

        public static void MaterialChanged(Material mat, UniToonVersion version, bool updateRenderQueue = true)
        {
            // clear keywords
            mat.shaderKeywords = new string[0];

            // normal map
            SetKeyword(mat, "_NORMALMAP", mat.GetTexture("_BumpMap") || mat.GetTexture("_DetailNormalMap"));

            // workflow mode
            if ((WorkflowMode)mat.GetFloat("_WorkflowMode") == WorkflowMode.Specular)
            {
                SetKeyword(mat, "_METALLICGLOSSMAP", mat.GetTexture("_SpecGlossMap"));
                SetKeyword(mat, "_SPECULAR_SETUP", true);
            }
            else
            {
                SetKeyword(mat, "_METALLICGLOSSMAP", mat.GetTexture("_MetallicGlossMap"));
            }
            
            // parallax map
            SetKeyword(mat, "_PARALLAXMAP", mat.GetTexture("_ParallaxMap"));

            // occlusion map
            SetKeyword(mat, "_OCCLUSIONMAP", mat.GetTexture("_OcclusionMap"));

            // detail map
            SetKeyword(mat, "_DETAIL_MULX2", mat.GetTexture("_DetailAlbedoMap") || mat.GetTexture("_DetailNormalMap"));

            // emission
            SetEmissionKeyword(mat);

            // smoothness texture channel
            SetKeyword(mat, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", (SmoothnessMapChannel)mat.GetFloat("_SmoothnessTextureChannel") == SmoothnessMapChannel.AlbedoAlpha);

            // specular highlights
            SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", mat.GetFloat("_SpecularHighlights") < 0.5f);

            // receive shadow
            SetKeyword(mat, "_RECEIVE_SHADOWS_OFF", mat.GetFloat("_ReceiveShadow") < 0.5f);

            // blend mode
            SetBlendMode(mat, updateRenderQueue);
        }

        private static void SetEmissionKeyword(Material mat)
        {
            var color = mat.GetColor("_EmissionColor");
            var realtimeEmission = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.RealtimeEmissive) > 0;
            SetKeyword(mat, "_EMISSION", color.maxColorComponent > 0.1f / 255.0f || realtimeEmission);
        }

        private static void SetBlendMode(Material mat, bool updateRenderQueue = true)
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
