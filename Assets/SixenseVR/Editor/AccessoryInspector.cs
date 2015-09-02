using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SixenseVR
{
    [CustomEditor(typeof(Accessory))]
    public class AccessoryInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var acc = target as Accessory;
            var obj = new SerializedObject(acc);

            EditorGUILayout.PropertyField(obj.FindProperty("m_pack"));
            EditorGUILayout.PropertyField(obj.FindProperty("m_offhand"));

            if (GUI.changed)
            {
                obj.ApplyModifiedProperties();
                obj.Update();
            }
        }
    }
}
