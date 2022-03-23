using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniToon.ShaderGUIs
{
    enum WorkflowMode
	{
		Specular,
		Metallic,
	}
	
	enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,
		Transparent,
        FadeWithZWrite,
		TransparentWithZWrite,
	}

	enum SmoothnessMapChannel
	{
		SpecularMetallicAlpha,
		AlbedoAlpha
	}

    enum RenderFace
    {
        Front = 2,
        Back = 1,
        Both = 0
    }

    class LitShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var mat = materialEditor.target as Material;

            // properties
            var propToonyFactor = FindProperty("_ToonyFactor", properties);

            // version
            GUILayout.Label("UniToon ver 0.1");

            // workflow
            BeginSection("Workflow");
            {
                var mode = (WorkflowMode)EditorGUILayout.EnumPopup("Mode", (WorkflowMode)mat.GetFloat("_WorkflowMode"));
                var shaderType = (BlendMode)EditorGUILayout.EnumPopup("Shader Type", (BlendMode)mat.GetFloat("_Blend"));
                var renderFace = (RenderFace)EditorGUILayout.EnumPopup("Render Face", (RenderFace)mat.GetFloat("_Cull"));
                var cutoff = mat.GetFloat("_Cutoff");
                if (shaderType == BlendMode.Cutout)
                {
                    cutoff = EditorGUILayout.Slider("Alpha Cutoff", cutoff, 0.0f, 1.0f);
                }
                var receiveShadow = EditorGUILayout.Toggle("Receive Shadow", mat.GetFloat("_ReceiveShadow") > 0.5f);
            
                if (EndSection())
                {
                    FindProperty("_WorkflowMode", properties).floatValue = (float)mode;
                    FindProperty("_Blend", properties).floatValue = (float)shaderType;
                    FindProperty("_Cull", properties).floatValue = (float)renderFace;
                    FindProperty("_Cutoff", properties).floatValue = cutoff;
                    FindProperty("_ReceiveShadow", properties).floatValue = receiveShadow ? 1.0f : 0.0f;

                    MaterialChanged(mat);
                }
            }

            // base color
            BeginSection("Base Color");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base"), FindProperty("_BaseMap", properties), FindProperty("_BaseColor", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Shade"), FindProperty("_ShadeMap", properties), FindProperty("_ShadeColor", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Emission"), FindProperty("_EmissionMap", properties), FindProperty("_EmissionColor", properties));

                materialEditor.TextureScaleOffsetProperty(FindProperty("_BaseMap", properties));
                
                if (EndSection())
                {
                    MaterialChanged(mat);
                }
            }

            // shading
            BeginSection("Shading");
            {
                var factor = EditorGUILayout.Slider("Toony Factor", mat.GetFloat("_ToonyFactor"), 0.001f, 1.0f);
                
                if (EndSection())
                {
                    FindProperty("_ToonyFactor", properties).floatValue = factor;

                    MaterialChanged(mat);
                }
            }

            // physical property
            BeginSection("Physical Property");
            {
                var mode = (WorkflowMode)mat.GetFloat("_WorkflowMode");
                if (mode == WorkflowMode.Metallic)
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Metallic"), FindProperty("_MetallicGlossMap", properties), FindProperty("_Metallic", properties));
                }
                else
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Specular"), FindProperty("_SpecGlossMap", properties), FindProperty("_SpecColor", properties));
                }
                var smoothness = EditorGUILayout.Slider("Smoothness", mat.GetFloat("_Smoothness"), 0.0f, 1.0f);
                var channel = (SmoothnessMapChannel)EditorGUILayout.EnumPopup("Smoothness Channel", (SmoothnessMapChannel)mat.GetFloat("_SmoothnessTextureChannel"));
                
                if (EndSection())
                {
                    FindProperty("_Smoothness", properties).floatValue = smoothness;
                    FindProperty("_SmoothnessTextureChannel", properties).floatValue = (float)channel;

                    MaterialChanged(mat);
                }
            }

            // surface
            BeginSection("Surface");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), FindProperty("_BumpMap", properties), mat.GetTexture("_BumpMap") ? FindProperty("_BumpScale", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Height"), FindProperty("_ParallaxMap", properties), mat.GetTexture("_ParallaxMap") ? FindProperty("_Parallax", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Occlusion"), FindProperty("_OcclusionMap", properties), mat.GetTexture("_OcclusionMap") ? FindProperty("_OcclusionStrength", properties) : null);
                
                if (EndSection())
                {
                    MaterialChanged(mat);
                }
            }

            // surface
            BeginSection("Detail");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Mask"), FindProperty("_DetailMask", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Albedo"), FindProperty("_DetailAlbedoMap", properties), mat.GetTexture("_DetailAlbedoMap") ? FindProperty("_DetailAlbedoMapScale", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Normal"), FindProperty("_DetailNormalMap", properties), mat.GetTexture("_DetailNormalMap") ? FindProperty("_DetailNormalMapScale", properties) : null);
                
                if (EndSection())
                {
                    MaterialChanged(mat);
                }
            }

            // outline (experimental)
            BeginSection("Outline (Experimental)");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Outline Color"), FindProperty("_OutlineMap", properties), FindProperty("_OutlineColor", properties));
                var width = EditorGUILayout.Slider("Outline Width", mat.GetFloat("_OutlineWidth"), 0.0f, 20.0f);
                var strength = EditorGUILayout.Slider("Outline Strength", mat.GetFloat("_OutlineStrength"), 0.0f, 1.0f);
                var smoothness = EditorGUILayout.Slider("Outline Smoothness", mat.GetFloat("_OutlineSmoothness"), 0.0f, 1.0f);
                
                if (EndSection())
                {
                    FindProperty("_OutlineWidth", properties).floatValue = width;
                    FindProperty("_OutlineStrength", properties).floatValue = strength;
                    FindProperty("_OutlineSmoothness", properties).floatValue = smoothness;

                    MaterialChanged(mat);
                }
            }

            // advance
            BeginSection("Advance");
            {
                materialEditor.RenderQueueField();

                var specHighlight = EditorGUILayout.Toggle("Specular Highlights", mat.GetFloat("_SpecularHighlights") > 0.5f);
                var envRef = EditorGUILayout.Toggle("Environment Reflections", mat.GetFloat("_EnvironmentReflections") > 0.5f);
                
                if (EndSection())
                {
                    FindProperty("_SpecularHighlights", properties).floatValue = specHighlight ? 1.0f : 0.0f;
                    FindProperty("_EnvironmentReflections", properties).floatValue = envRef ? 1.0f : 0.0f;

                    MaterialChanged(mat, false);
                }
            }
        }

        private static void MaterialChanged(Material mat, bool updateRenderQueue = true)
        {
            // clear keywords
            mat.shaderKeywords = new string[0];

            // normal map
            SetKeyword(mat, "_NORMALMAP", mat.GetTexture("_BumpMap") || mat.GetTexture("_DetailNormalMap"));

            // workflow mode
            if ((WorkflowMode)mat.GetFloat("_WorkflowMode") == WorkflowMode.Specular)
            {
                SetKeyword(mat, "_SPECGLOSSMAP", mat.GetTexture("_SpecGlossMap"));
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

        private static void BeginSection(string name)
        {
            EditorGUILayout.Space();
            GUILayout.Label(name, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
        }

        private static bool EndSection()
        {
            EditorGUI.indentLevel--;
            return EditorGUI.EndChangeCheck();
        }
    }

}

