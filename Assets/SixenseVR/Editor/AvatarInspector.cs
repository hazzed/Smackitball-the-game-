using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SixenseVR
{
    [CustomEditor(typeof(Avatar))]
    public class AvatarInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var avatar = target as SixenseVR.Avatar;
            var obj = new SerializedObject(target);

            EditorGUILayout.PropertyField(obj.FindProperty("m_animationVariables"), true);

            EditorGUILayout.PropertyField(obj.FindProperty("m_rootMotion"));

            {
                var prop = obj.FindProperty("m_joystickLocomotion");
                EditorGUILayout.PropertyField(prop);

                if (prop.boolValue)
                {
                    EditorGUILayout.PropertyField(obj.FindProperty("m_walkSpeed"));
                    EditorGUILayout.PropertyField(obj.FindProperty("m_turnSpeed"));
                }
                else
                {
                    EditorGUILayout.PropertyField(obj.FindProperty("m_minimumSafeAreaRadius"));
                    EditorGUILayout.PropertyField(obj.FindProperty("m_safeAreaFollowsTransform"));
                }
            }
            EditorGUILayout.PropertyField(obj.FindProperty("m_autoGrabAccessories"), true);

            if (GUI.changed)
            {
                obj.ApplyModifiedProperties();
                obj.Update();
            }

            if (GUILayout.Button("Edit Avatar Definition"))
            {
                var window = CreateAvatarWindow.Init();

                window.m_selectedAvatar = avatar.gameObject;
            }
        }
    }
}