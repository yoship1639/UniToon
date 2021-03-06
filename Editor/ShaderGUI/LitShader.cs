using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniToon
{
    class LitShader : ShaderGUI
    {
        private bool firstTime = true;
        SavedBool fo_workflow;
        SavedBool fo_baseColor;
        SavedBool fo_shading;
        SavedBool fo_normalCorrection;
        SavedBool fo_shadowCorrection;
        SavedBool fo_physicalProperty;
        SavedBool fo_surface;
        SavedBool fo_detail;
        SavedBool fo_outline;
        SavedBool fo_postprocessing;
        SavedBool fo_advanced;

        private Transform normalCorrectOriginTrans;
        private Transform shadowCorrectOriginTrans;

        void OnOpenGUI()
        {
            var prefix = "UniToonLitShaderGUI";
            fo_workflow = new SavedBool($"{prefix}.Workflow", true);
            fo_baseColor = new SavedBool($"{prefix}.BaseColor", true);
            fo_shading = new SavedBool($"{prefix}.Shading", true);
            fo_normalCorrection = new SavedBool($"{prefix}.NormalCorrection", true);
            fo_shadowCorrection = new SavedBool($"{prefix}.ShadowCorrection", true);
            fo_physicalProperty = new SavedBool($"{prefix}.PhysicalProperty", true);
            fo_surface = new SavedBool($"{prefix}.Surface", true);
            fo_detail = new SavedBool($"{prefix}.Detail", true);
            fo_outline = new SavedBool($"{prefix}.Outline", true);
            fo_postprocessing = new SavedBool($"{prefix}.PostProcessing", true);
            fo_advanced = new SavedBool($"{prefix}.Advanced", true);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var mat = materialEditor.target as Material;

            if (firstTime)
            {
                OnOpenGUI();
                firstTime = false;
            }

            var changed = false;
            var blendModeChanged = false;

            if (GUILayout.Button("Documents", GUILayout.Width(120)))
            {
                System.Diagnostics.Process.Start("https://yoship1639.github.io/UniToon/Documents/documents.html");
            }

            EditorGUILayout.Space();

            // version
            GUILayout.Label("UniToon ver 0.22.0-alpha");

            EditorGUILayout.Space();
            changed = MaterialGUI.Enum<UniToonVersion>("Version", FindProperty("_UniToonVer", properties));
            var ver = ((UniToonVersion)mat.GetInt("_UniToonVer"));
            if (changed)
            {
                var shader = Shader.Find($"UniToon/{ver.ToString()}/Lit");
                if (shader == null)
                {
                    Debug.LogError($"UniToon/{ver.ToString()}/Lit shader not found.");
                    return;
                }
                foreach (Material m in materialEditor.targets)
                {
                    m.shader = shader;
                    MaterialConverter.MaterialChanged(m, ver);
                }
            }

            // workflow
            if (BeginSection("Workflow", fo_workflow))
            {
                changed |= MaterialGUI.Enum<WorkflowMode>("Mode", FindProperty("_WorkflowMode", properties));
                var prevBlend = (BlendMode)FindProperty("_Blend", properties).floatValue;
                changed |= MaterialGUI.Enum<BlendMode>("Shader Type", FindProperty("_Blend", properties));
                var newBlend = (BlendMode)FindProperty("_Blend", properties).floatValue;
                blendModeChanged = prevBlend != newBlend;
                changed |= MaterialGUI.Enum<RenderFace>("Render Face", FindProperty("_Cull", properties));

                if (newBlend != BlendMode.Opaque)
                {
                    changed |= MaterialGUI.Slider("Alpha Cutoff", FindProperty("_Cutoff", properties), 0.0f, 1.0f);
                }
                changed |= MaterialGUI.Toggle("Receive Shadow", FindProperty("_ReceiveShadow", properties));
            }
            changed |= EndSection();

            // base color
            if (BeginSection("Base Color", fo_baseColor))
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base"), FindProperty("_BaseMap", properties), FindProperty("_BaseColor", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Shade"), FindProperty("_ShadeMap", properties), FindProperty("_ShadeColor", properties));
                changed |= MaterialGUI.Slider("Shade Hue", FindProperty("_ShadeHue", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Shade Saturation", FindProperty("_ShadeSaturation", properties), 0.0f, 4.0f);
                changed |= MaterialGUI.Slider("Shade Brightness", FindProperty("_ShadeBrightness", properties), 0.0f, 1.0f);
                materialEditor.TexturePropertySingleLine(new GUIContent("Emission"), FindProperty("_EmissionMap", properties), FindProperty("_EmissionColor", properties));
                materialEditor.TextureScaleOffsetProperty(FindProperty("_BaseMap", properties));
            }
            changed |= EndSection();

            // shading
            if (BeginSection("Shading", fo_shading))
            {
                changed |= MaterialGUI.Slider("Toony Factor", FindProperty("_ToonyFactor", properties), 0.001f, 1.0f);
            }
            changed |= EndSection();

            if (BeginSection("Spherical Normal Correction (Experimental)", fo_normalCorrection))
            {
                changed |= MaterialGUI.Slider("Factor", FindProperty("_NormalCorrect", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Vector3("Origin", FindProperty("_NormalCorrectOrigin", properties));
                // normalCorrectOriginTrans = (Transform)EditorGUILayout.ObjectField("[Editor Only] Origin Transform", normalCorrectOriginTrans, typeof(Transform), true);
                // if (normalCorrectOriginTrans != null && GUILayout.Button("Calc From Origin Transform"))
                // {
                //     FindProperty("_NormalCorrectOrigin", properties).vectorValue = normalCorrectOriginTrans.localPosition;
                //     changed = true;
                // }
            }
            changed |= EndSection();

            // shadow correction
            if (BeginSection("Spherical Shadow Correction (Experimental)", fo_shadowCorrection))
            {
                changed |= MaterialGUI.Slider("Factor", FindProperty("_ShadowCorrect", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Vector3("Origin", FindProperty("_ShadowCorrectOrigin", properties));
                changed |= MaterialGUI.Slider("Radius", FindProperty("_ShadowCorrectRadius", properties), 0.0f, 2.0f);
            }
            changed |= EndSection();

            // physical property
            if (BeginSection("Physical Property", fo_physicalProperty))
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
                changed |= MaterialGUI.Slider("Smoothness", FindProperty("_Smoothness", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Enum<SmoothnessMapChannel>("Smoothness Channel", FindProperty("_SmoothnessTextureChannel", properties));
            }
            changed |= EndSection();

            // surface
            if (BeginSection("Surface", fo_surface))
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), FindProperty("_BumpMap", properties), mat.GetTexture("_BumpMap") ? FindProperty("_BumpScale", properties) : null);
#if UNITY_2020_2_OR_NEWER
                materialEditor.TexturePropertySingleLine(new GUIContent("Height"), FindProperty("_ParallaxMap", properties), mat.GetTexture("_ParallaxMap") ? FindProperty("_Parallax", properties) : null);
#endif
                materialEditor.TexturePropertySingleLine(new GUIContent("Occlusion"), FindProperty("_OcclusionMap", properties), mat.GetTexture("_OcclusionMap") ? FindProperty("_OcclusionStrength", properties) : null);
            }
            changed |= EndSection();

#if UNITY_2020_2_OR_NEWER
            // detail
            if (BeginSection("Detail", fo_detail))
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Mask"), FindProperty("_DetailMask", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Albedo"), FindProperty("_DetailAlbedoMap", properties), mat.GetTexture("_DetailAlbedoMap") ? FindProperty("_DetailAlbedoMapScale", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Normal"), FindProperty("_DetailNormalMap", properties), mat.GetTexture("_DetailNormalMap") ? FindProperty("_DetailNormalMapScale", properties) : null);
            }
            changed |= EndSection();
#endif

            // outline
            if (BeginSection("Outline", fo_outline))
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Outline Mask"), FindProperty("_OutlineMask", properties));
                changed |= MaterialGUI.Slider("Outline Saturation", FindProperty("_OutlineSaturation", properties), 0.0f, 4.0f);
                changed |= MaterialGUI.Slider("Outline Brightness", FindProperty("_OutlineBrightness", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Outline Light Affects", FindProperty("_OutlineLightAffects", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Outline Width", FindProperty("_OutlineWidth", properties), 0.0f, 20.0f);
                changed |= MaterialGUI.Slider("Outline Strength", FindProperty("_OutlineStrength", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Outline Smoothness", FindProperty("_OutlineSmoothness", properties), 0.0f, 1.0f);

                EditorGUILayout.HelpBox("Outline requires that the camera depth texture is enabled", MessageType.Info);
            }
            changed |= EndSection();

            // post process
            if (BeginSection("Post-Processing", fo_postprocessing))
            {
                changed |= MaterialGUI.Slider("Diffuse Intensity", FindProperty("_PostDiffuseIntensity", properties), 0.0f, 2.0f);
                changed |= MaterialGUI.Slider("Specular Intensity", FindProperty("_PostSpecularIntensity", properties), 0.0f, 2.0f);
                changed |= MaterialGUI.Slider("GI Intensity", FindProperty("_PostGIIntensity", properties), 0.0f, 2.0f);
                changed |= MaterialGUI.Slider("Min Brightness", FindProperty("_PostMinBrightness", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("MainLight Hi-Cut Intensity", FindProperty("_MainLightHiCut", properties), 0.0f, 10.0f);
                changed |= MaterialGUI.Slider("AdditionalLight Hi-Cut Intensity", FindProperty("_AdditionalLightHiCut", properties), 0.0f, 10.0f);

                EditorGUILayout.HelpBox("Basically, it is recommended not to change the post-processing values", MessageType.Info);
            }
            changed |= EndSection();

            // advance
            if (BeginSection("Advanced", fo_advanced))
            {
                materialEditor.RenderQueueField();
                changed |= MaterialGUI.Toggle("Specular Highlights", FindProperty("_SpecularHighlights", properties));
                changed |= MaterialGUI.Toggle("Environment Reflections", FindProperty("_EnvironmentReflections", properties));
                materialEditor.EnableInstancingField();
            }
            changed |= EndSection();

            if (changed || blendModeChanged)
            {
                foreach (Material m in materialEditor.targets)
                {
                    MaterialConverter.MaterialChanged(m, ver, blendModeChanged);
                }
            }
        }

        private static bool BeginSection(string name, SavedBool foldout)
        {
            EditorGUILayout.Space();
            foldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(foldout.value, name, EditorStyles.foldoutHeader);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            return foldout.value;
        }

        private static bool EndSection()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel--;
            return EditorGUI.EndChangeCheck();
        }

        
    }

}

