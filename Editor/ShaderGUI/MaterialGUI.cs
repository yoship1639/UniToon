using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniToon
{
    static class MaterialGUI
    {
        public static bool Toggle(string label, MaterialProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var val = EditorGUILayout.Toggle(label, prop.floatValue > 0.5f);
            EditorGUI.showMixedValue = false;
            var res = EditorGUI.EndChangeCheck();
            if (res) prop.floatValue = val ? 1.0f : 0.0f;
            return res;
        }

        public static bool Slider(string label, MaterialProperty prop, float min, float max)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var val = EditorGUILayout.Slider(label, prop.floatValue, min, max);
            EditorGUI.showMixedValue = false;
            var res = EditorGUI.EndChangeCheck();
            if (res) prop.floatValue = val;
            return res;
        }

        public static bool Vector3(string label, MaterialProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var val = EditorGUILayout.Vector3Field(label, prop.vectorValue);
            EditorGUI.showMixedValue = false;
            var res = EditorGUI.EndChangeCheck();
            if (res) prop.vectorValue = val;
            return res;
        }

        public static bool Color(string label, MaterialProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var val = EditorGUILayout.ColorField(label, prop.colorValue);
            EditorGUI.showMixedValue = false;
            var res = EditorGUI.EndChangeCheck();
            if (res) prop.colorValue = val;
            return res;
        }

        public static bool Enum<T>(string label, MaterialProperty prop) where T : Enum
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var val = (T)System.Enum.ToObject(typeof(T), (int)prop.floatValue);
            val = (T)EditorGUILayout.EnumPopup(label, val);
            EditorGUI.showMixedValue = false;
            var res = EditorGUI.EndChangeCheck();
            if (res) prop.floatValue = Convert.ToInt32(val);
            return res;
        }
    }
}
