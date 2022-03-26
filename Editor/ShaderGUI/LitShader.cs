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

            // properties
            var propToonyFactor = FindProperty("_ToonyFactor", properties);

            // version
            GUILayout.Label("UniToon ver 0.3.0");

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            var ver = UniToonVersion.URP_2021_2;
            if (mat.shader.name.Contains(UniToonVersion.URP_2021_2.ToString()))
            {
                ver = UniToonVersion.URP_2021_2;
            }
            ver = (UniToonVersion)EditorGUILayout.EnumPopup("Version", ver);
            if (EditorGUI.EndChangeCheck())
            {
                var shader = Shader.Find($"UniToon/{ver.ToString()}/Lit");
                if (shader == null)
                {
                    Debug.LogError($"UniToon/{ver.ToString()}/Lit shader not found.");
                    return;
                }
                mat.shader = shader;
                MaterialConverter.MaterialChanged(mat, ver);
            }

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

                    MaterialConverter.MaterialChanged(mat, ver);
                }
            }

            // base color
            BeginSection("Base Color");
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base"), FindProperty("_BaseMap", properties), FindProperty("_BaseColor", properties));
                materialEditor.TexturePropertySingleLine(new GUIContent("Shade"), FindProperty("_ShadeMap", properties), FindProperty("_ShadeColor", properties));
                var h = EditorGUILayout.Slider("Shade Hue", mat.GetFloat("_ShadeHue"), 0.0f, 1.0f);
                var s = EditorGUILayout.Slider("Shade Saturation", mat.GetFloat("_ShadeSaturation"), 0.0f, 4.0f);
                var v = EditorGUILayout.Slider("Shade Brightness", mat.GetFloat("_ShadeBrightness"), 0.0f, 1.0f);
                materialEditor.TexturePropertySingleLine(new GUIContent("Emission"), FindProperty("_EmissionMap", properties), FindProperty("_EmissionColor", properties));

                materialEditor.TextureScaleOffsetProperty(FindProperty("_BaseMap", properties));
                
                if (EndSection())
                {
                    FindProperty("_ShadeHue", properties).floatValue = h;
                    FindProperty("_ShadeSaturation", properties).floatValue = s;
                    FindProperty("_ShadeBrightness", properties).floatValue = v;

                    MaterialConverter.MaterialChanged(mat, ver);
                }
            }

            // shading
            BeginSection("Shading");
            {
                var factor = EditorGUILayout.Slider("Toony Factor", mat.GetFloat("_ToonyFactor"), 0.001f, 1.0f);
                
                if (EndSection())
                {
                    FindProperty("_ToonyFactor", properties).floatValue = factor;

                    MaterialConverter.MaterialChanged(mat, ver);
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

                    MaterialConverter.MaterialChanged(mat, ver);
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
                    MaterialConverter.MaterialChanged(mat, ver);
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
                    MaterialConverter.MaterialChanged(mat, ver);
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

                    MaterialConverter.MaterialChanged(mat, ver);
                }

                EditorGUILayout.HelpBox("Outline (Experimental) requires that the camera depth texture is enabled", MessageType.Info);
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

                    MaterialConverter.MaterialChanged(mat, ver, false);
                }
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

