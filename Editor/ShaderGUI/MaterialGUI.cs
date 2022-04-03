using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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

    class SavedParameter<T>
            where T : IEquatable<T>
        {
            internal delegate void SetParameter(string key, T value);
            internal delegate T GetParameter(string key, T defaultValue);

            readonly string m_Key;
            bool m_Loaded;
            T m_Value;

            readonly SetParameter m_Setter;
            readonly GetParameter m_Getter;

            public SavedParameter(string key, T value, GetParameter getter, SetParameter setter)
            {
                Assert.IsNotNull(setter);
                Assert.IsNotNull(getter);

                m_Key = key;
                m_Loaded = false;
                m_Value = value;
                m_Setter = setter;
                m_Getter = getter;
            }

            void Load()
            {
                if (m_Loaded)
                    return;

                m_Loaded = true;
                m_Value = m_Getter(m_Key, m_Value);
            }

            public T value
            {
                get
                {
                    Load();
                    return m_Value;
                }
                set
                {
                    Load();

                    if (m_Value.Equals(value))
                        return;

                    m_Value = value;
                    m_Setter(m_Key, value);
                }
            }
        }

        // Pre-specialized class for easier use and compatibility with existing code
        sealed class SavedBool : SavedParameter<bool>
        {
            public SavedBool(string key, bool value)
                : base(key, value, EditorPrefs.GetBool, EditorPrefs.SetBool) { }
        }
}
