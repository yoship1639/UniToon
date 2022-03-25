using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniToon
{
    public class MMD2UniToon
    {
        private const string MenuItemName = "Assets/UniToon/MMD to UniToon";

        [MenuItem(MenuItemName, false)]
        public static void MMD2UniToonMenu()
        {
            if (!IsEnabledMMD2UniToonMenu())
            {
                return;
            }
            const string progressBarTitle = "[MMD2UniToon] MMD to UniToon";

            int assetCount = Selection.assetGUIDs.Length;
            for (int i = 0; i < assetCount; ++i)
            {
                string guid = Selection.assetGUIDs[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                if (mat == null || mat.shader == null || !mat.shader.name.Contains("MMD4Mecanim"))
                {
                    continue;
                }
                EditorUtility.DisplayProgressBar(progressBarTitle, string.Format("{0}", mat.name), i / (float)assetCount);
                ConvertToUniToon(mat);
            }

            EditorUtility.DisplayProgressBar(progressBarTitle, "[MMD2UniToon] Refresh AssetDatabase...", 1f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(MenuItemName, true)]
        static bool IsEnabledMMD2UniToonMenu()
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
                if (mat == null || mat.shader == null || !mat.shader.name.Contains("MMD4Mecanim"))
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
            var bothFace = mat.shader.name.Contains("BothFaces");
            var transparent = mat.shader.name.Contains("Transparent");
            var noShadow = mat.shader.name.Contains("NoShadowCasting");

            mat.shader = unitoon;

            mat.SetTexture("_BaseMap", baseMap);
            mat.SetTexture("_ShadeMap", baseMap);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Cull", bothFace ? 0.0f : 2.0f);
            mat.SetFloat("_Blend", transparent ? 4.0f : 0.0f);
            mat.SetFloat("_ReceiveShadow", noShadow ? 0.0f : 1.0f);

            MaterialConverter.MaterialChanged(mat, ver);
        }
    }
}
