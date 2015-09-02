using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SixenseVR
{
    [CustomEditor(typeof(GrabPoint))]
    public class GrabPointInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var grab = target as GrabPoint;
            var obj = new SerializedObject(grab);

            EditorGUILayout.PropertyField(obj.FindProperty("m_handedness"));
            EditorGUILayout.PropertyField(obj.FindProperty("m_animationTransition"));

            EditorGUILayout.PropertyField(obj.FindProperty("m_grabRange"));
            EditorGUILayout.PropertyField(obj.FindProperty("m_grabMaxAngle"));
            EditorGUILayout.PropertyField(obj.FindProperty("m_radialSymmetry"));

            EditorGUILayout.PropertyField(obj.FindProperty("m_dropOnRelease"));

            if (GUI.changed)
            {
                obj.ApplyModifiedProperties();
                obj.Update();
            }
        }
    }
}
