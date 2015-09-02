using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SixenseVR
{
    [CustomEditor(typeof(FixedBase))]
    public class FixedBaseInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var fb = target as FixedBase;
            var obj = new SerializedObject(fb);

            EditorGUILayout.PropertyField(obj.FindProperty("m_safeFloor"));

            if (GUI.changed)
            {
                obj.ApplyModifiedProperties();
                obj.Update();
            }
        }
    }
}
