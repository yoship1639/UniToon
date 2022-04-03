using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniToon
{
    public class MToon2UniToon
    {
        private const string MenuItemName = "Assets/UniToon/MToon to UniToon";

        [MenuItem(MenuItemName, false)]
        public static void MToon2UniToonMenu()
        {
            if (!IsEnabledMToon2UniToonMenu())
            {
                return;
            }
            const string progressBarTitle = "[MToon2UniToon] URP Lit to UniToon";

            int assetCount = Selection.assetGUIDs.Length;
            for (int i = 0; i < assetCount; ++i)
            {
                string guid = Selection.assetGUIDs[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                if (mat == null || mat.shader == null || mat.shader.name != "VRM/MToon")
                {
                    continue;
                }
                EditorUtility.DisplayProgressBar(progressBarTitle, string.Format("{0}", mat.name), i / (float)assetCount);
                ConvertToUniToon(mat);
            }

            EditorUtility.DisplayProgressBar(progressBarTitle, "[MToon2UniToon] Refresh AssetDatabase...", 1f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(MenuItemName, true)]
        static bool IsEnabledMToon2UniToonMenu()
        {
            if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
            {
                return false;
            }
            int assetCount = Selection.assetGUIDs.Length;
            for (int i = 0; i < assetCount; ++i)
            {
                string guid = Selection.assetGUIDs[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                if (mat == null || mat.shader == null || mat.shader.name != "VRM/MToon")
                {
                    return false;
                }
            }
            return true;
        }

        static void ConvertToUniToon(Material mat)
        {
            var ver = MaterialConverter.GetCurrentVersion();
            var unitoon = Shader.Find($"UniToon/{ver}/Lit");
            if (unitoon == null)
            {
                return;
            }

            var baseMap = mat.GetTexture("_MainTex");
            var baseColor = mat.GetColor("_Color");
            var shadeMap = mat.GetTexture("_ShadeTexture");
            var toony = 1.0f - mat.GetFloat("_ShadeToony");
            var receiveShadow = mat.GetFloat("_ReceiveShadowRate") >= 0.5f;
            var cull = mat.GetFloat("_CullMode");
            var srcBlend = (UnityEngine.Rendering.BlendMode)mat.GetFloat("_SrcBlend");
            var dstBlend = (UnityEngine.Rendering.BlendMode)mat.GetFloat("_DstBlend");
            var cutout = mat.IsKeywordEnabled("_ALPHATEST_ON");
            var zwrite = mat.GetFloat("_ZWrite") >= 0.5f;
            var outlineMask = mat.GetTexture("_OutlineWidthTexture");
            var outlineWidth = mat.GetFloat("_OutlineWidth") * 20.0f;

            mat.shader = unitoon;

            mat.SetTexture("_BaseMap", baseMap);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetTexture("_ShadeMap", shadeMap != null ? shadeMap : baseMap);
            mat.SetFloat("_ToonyFactor", toony);
            mat.SetFloat("_ReceiveShadow", receiveShadow ? 1.0f :0.0f);
            mat.SetFloat("_Cull", cull);
            mat.SetTexture("_OutlineMask", outlineMask);
            mat.SetFloat("_OutlineWidth", outlineWidth);

            if (srcBlend == UnityEngine.Rendering.BlendMode.One && dstBlend == UnityEngine.Rendering.BlendMode.Zero)
            {
                if (cutout) mat.SetFloat("_Blend", (float)BlendMode.Cutout);
                else mat.SetFloat("_Blend", (float)BlendMode.Opaque);
            }
            else if (srcBlend == UnityEngine.Rendering.BlendMode.SrcAlpha && dstBlend == UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha)
            {
                if (zwrite) mat.SetFloat("_Blend", (float)BlendMode.FadeWithZWrite);
                else mat.SetFloat("_Blend", (float)BlendMode.Fade);
            }
            else if (srcBlend == UnityEngine.Rendering.BlendMode.One && dstBlend == UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha)
            {
                if (zwrite) mat.SetFloat("_Blend", (float)BlendMode.TransparentWithZWrite);
                else mat.SetFloat("_Blend", (float)BlendMode.Transparent);
            }

            MaterialConverter.MaterialChanged(mat, ver);
        }
    }
}
