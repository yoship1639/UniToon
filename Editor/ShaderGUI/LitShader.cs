using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniToon
{
    class LitShader : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var mat = materialEditor.target as Material;

            var changed = false;

            // version
            GUILayout.Label("UniToon ver 0.15.0-alpha");

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
            BeginSection("Workflow");
            {
                changed |= MaterialGUI.Enum<WorkflowMode>("Mode", FindProperty("_WorkflowMode", properties));
                changed |= MaterialGUI.Enum<BlendMode>("Shader Type", FindProperty("_Blend", properties));
                changed |= MaterialGUI.Enum<RenderFace>("Render Face", FindProperty("_Cull", properties));

                if ((BlendMode)mat.GetFloat("_Blend") == BlendMode.Cutout)
                {
                    changed |= MaterialGUI.Slider("Alpha Cutoff", FindProperty("_Cutoff", properties), 0.0f, 1.0f);
                }
                changed |= MaterialGUI.Toggle("Receive Shadow", FindProperty("_ReceiveShadow", properties));

                changed |= EndSection();
            }

            // base color
            BeginSection("Base Color");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base"), FindProperty("_BaseMap", properties), FindProperty("_BaseColor", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Shade"), FindProperty("_ShadeMap", properties), FindProperty("_ShadeColor", properties));
                changed |= MaterialGUI.Slider("Shade Hue", FindProperty("_ShadeHue", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Shade Saturation", FindProperty("_ShadeSaturation", properties), 0.0f, 4.0f);
                changed |= MaterialGUI.Slider("Shade Brightness", FindProperty("_ShadeBrightness", properties), 0.0f, 1.0f);
                materialEditor.TexturePropertySingleLine(new GUIContent("Emission"), FindProperty("_EmissionMap", properties), FindProperty("_EmissionColor", properties));
                materialEditor.TextureScaleOffsetProperty(FindProperty("_BaseMap", properties));
                
                changed |= EndSection();
            }

            // shading
            BeginSection("Shading");
            {
                changed |= MaterialGUI.Slider("Toony Factor", FindProperty("_ToonyFactor", properties), 0.001f, 1.0f);
                changed |= MaterialGUI.Slider("Spherical Normal Correct", FindProperty("_NormalCorrect", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Vector3("Spherical Normal Correct Origin", FindProperty("_NormalCorrectOrigin", properties));

                changed |= EndSection();
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
                changed |= MaterialGUI.Slider("Smoothness", FindProperty("_Smoothness", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Enum<SmoothnessMapChannel>("Smoothness Channel", FindProperty("_SmoothnessTextureChannel", properties));
                
                changed |= EndSection();
            }

            // surface
            BeginSection("Surface");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Normal"), FindProperty("_BumpMap", properties), mat.GetTexture("_BumpMap") ? FindProperty("_BumpScale", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Height"), FindProperty("_ParallaxMap", properties), mat.GetTexture("_ParallaxMap") ? FindProperty("_Parallax", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Occlusion"), FindProperty("_OcclusionMap", properties), mat.GetTexture("_OcclusionMap") ? FindProperty("_OcclusionStrength", properties) : null);
                
                changed |= EndSection();
            }

            // detail
            BeginSection("Detail");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Mask"), FindProperty("_DetailMask", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Albedo"), FindProperty("_DetailAlbedoMap", properties), mat.GetTexture("_DetailAlbedoMap") ? FindProperty("_DetailAlbedoMapScale", properties) : null);
                materialEditor.TexturePropertySingleLine(new GUIContent("Detail Normal"), FindProperty("_DetailNormalMap", properties), mat.GetTexture("_DetailNormalMap") ? FindProperty("_DetailNormalMapScale", properties) : null);
                
                changed |= EndSection();
            }

            // outline (experimental)
            BeginSection("Outline (Experimental)");
            {
                changed |= MaterialGUI.Slider("Outline Saturation", FindProperty("_OutlineSaturation", properties), 0.0f, 4.0f);
                changed |= MaterialGUI.Slider("Outline Brightness", FindProperty("_OutlineBrightness", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Outline Light Affects", FindProperty("_OutlineLightAffects", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Outline Width", FindProperty("_OutlineWidth", properties), 0.0f, 20.0f);
                changed |= MaterialGUI.Slider("Outline Strength", FindProperty("_OutlineStrength", properties), 0.0f, 1.0f);
                changed |= MaterialGUI.Slider("Outline Smoothness", FindProperty("_OutlineSmoothness", properties), 0.0f, 1.0f);

                EditorGUILayout.HelpBox("Outline (Experimental) requires that the camera depth texture is enabled", MessageType.Info);
                changed |= EndSection();
            }

            // post process
            BeginSection("Post-Processing");
            {
                changed |= MaterialGUI.Slider("Diffuse Intensity", FindProperty("_PostDiffuseIntensity", properties), 0.0f, 2.0f);
                changed |= MaterialGUI.Slider("Specular Intensity", FindProperty("_PostSpecularIntensity", properties), 0.0f, 2.0f);
                changed |= MaterialGUI.Slider("GI Intensity", FindProperty("_PostGIIntensity", properties), 0.0f, 2.0f);
                changed |= MaterialGUI.Slider("MainLight Hi-Cut Intensity", FindProperty("_MainLightHiCut", properties), 0.0f, 10.0f);
                changed |= MaterialGUI.Slider("AdditionalLight Hi-Cut Intensity", FindProperty("_AdditionalLightHiCut", properties), 0.0f, 10.0f);

                EditorGUILayout.HelpBox("Basically, it is recommended not to change the post-processing values", MessageType.Info);
                changed |= EndSection();
            }

            // advance
            BeginSection("Advance");
            {
                materialEditor.RenderQueueField();
                changed |= MaterialGUI.Toggle("Specular Highlights", FindProperty("_SpecularHighlights", properties));
                changed |= MaterialGUI.Toggle("Environment Reflections", FindProperty("_EnvironmentReflections", properties));
                materialEditor.EnableInstancingField();
                
                changed |= EndSection();
            }

            if (changed)
            {
                MaterialConverter.MaterialChanged(mat, ver);
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

