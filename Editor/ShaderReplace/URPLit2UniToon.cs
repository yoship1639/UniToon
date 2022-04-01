using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniToon
{
    public class URPLit2UniToon
    {
        private const string MenuItemName = "Assets/UniToon/URP Lit to UniToon";

        [MenuItem(MenuItemName, false)]
        public static void URPLit2UniToonMenu()
        {
            if (!IsEnabledURPLit2UniToonMenu())
            {
                return;
            }
            const string progressBarTitle = "[URPLit2UniToon] URP Lit to UniToon";

            int assetCount = Selection.assetGUIDs.Length;
            for (int i = 0; i < assetCount; ++i)
            {
                string guid = Selection.assetGUIDs[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                if (mat == null || mat.shader == null || mat.shader.name != "Universal Render Pipeline/Lit")
                {
                    continue;
                }
                EditorUtility.DisplayProgressBar(progressBarTitle, string.Format("{0}", mat.name), i / (float)assetCount);
                ConvertToUniToon(mat);
            }

            EditorUtility.DisplayProgressBar(progressBarTitle, "[URPLit2UniToon] Refresh AssetDatabase...", 1f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem(MenuItemName, true)]
        static bool IsEnabledURPLit2UniToonMenu()
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
                if (mat == null || mat.shader == null || mat.shader.name != "Universal Render Pipeline/Lit")
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

            var baseMap = mat.GetTexture("_BaseMap");

            mat.shader = unitoon;

            mat.SetTexture("_ShadeMap", baseMap);

            MaterialConverter.MaterialChanged(mat, ver);
        }
    }
}
