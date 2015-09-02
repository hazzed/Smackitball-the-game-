//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SixenseVR
{
    public class CreateAvatarWindow : EditorWindow
    {
        #region Public Variables
        public GameObject m_selectedAvatar;
        #endregion

        #region Private Variables
        Object m_selectedModel;
        bool m_bInitialized;

        Transform m_mirrorRoot;
        Transform m_mirrorFirst;
        Transform m_mirrorSecond;
        bool m_mirrorEnabled = false;
        Vector2 m_scrollPosition;

        bool m_modelSelect;

        int m_doRefresh;

        static bool ms_hasVersion;

        static string version = "";

        List<GameObject> AvatarObjects = new List<GameObject>();
        #endregion

        #region Helper Functions
        void GetCompatibleHumanoids()
        {
            string[] paths = Directory.GetFiles(Application.dataPath, "*.fbx", SearchOption.AllDirectories);

            AvatarObjects.Clear();

            foreach (string path in paths)
            {
                string assetPath = "Assets" + path.Replace(Application.dataPath, "");
                ModelImporter model = ModelImporter.GetAtPath(assetPath) as ModelImporter;

                if (model != null && model.animationType == ModelImporterAnimationType.Human)
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

                    if (obj.GetComponent<Animator>())
                    {
                        AvatarObjects.Add(obj);
                    }
                }
            }
        }
        #endregion

        #region Avatar Setup
        private void NewAvatar()
        {
            Object avatarPrefab = AssetDatabase.LoadAssetAtPath("Assets/SixenseVR/Prefabs/SixenseAvatar.prefab", typeof(UnityEngine.Object));
            GameObject avatar = PrefabUtility.InstantiatePrefab(avatarPrefab) as GameObject;
            GameObject playerModel = PrefabUtility.InstantiatePrefab(m_selectedModel) as GameObject;

            if (playerModel != null)
            {
                playerModel.transform.parent = avatar.transform;
                playerModel.transform.localPosition = Vector3.zero;
                playerModel.transform.localRotation = Quaternion.identity;

                Animator objAnimator = (m_selectedModel as GameObject).GetComponent<Animator>();
                Animator animator = avatar.GetComponent<Animator>();

                if (animator != null)
                {
                    animator.avatar = objAnimator.avatar;
                }

                var avatarControl = avatar.GetComponent<Avatar>();

                m_mirrorFirst = avatarControl.m_targetL;
                m_mirrorSecond = avatarControl.m_targetR;
                m_mirrorRoot = avatarControl.transform;
                m_mirrorEnabled = true;

                avatar.name += "_" + playerModel.name;
            }

            Selection.activeObject = avatar;
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.RepaintAll();

            m_selectedAvatar = avatar;

            UpdateAvatar();

            {
                var avatarControl = avatar.GetComponent<Avatar>();
                VRCameraSetup.CreateCamera(avatarControl.m_cameraMount);
            }
        }

        private void SetModel()
        {
            foreach (Transform t in m_selectedAvatar.transform)
            {
                if (t.GetComponent<Animator>() != null)
                {
                    DestroyImmediate(t.gameObject);
                    break;
                }
            }

            GameObject playerModel = PrefabUtility.InstantiatePrefab(m_selectedModel) as GameObject;

            if (playerModel != null)
            {
                playerModel.transform.parent = m_selectedAvatar.transform;
                playerModel.transform.localPosition = Vector3.zero;
                playerModel.transform.localRotation = Quaternion.identity;

                Animator objAnimator = (m_selectedModel as GameObject).GetComponent<Animator>();
                Animator animator = m_selectedAvatar.GetComponent<Animator>();

                if (animator != null)
                {
                    animator.avatar = objAnimator.avatar;
                }

                var avatarControl = m_selectedAvatar.GetComponent<Avatar>();

                m_mirrorFirst = avatarControl.m_targetL;
                m_mirrorSecond = avatarControl.m_targetR;
                m_mirrorRoot = avatarControl.transform;
                m_mirrorEnabled = true;
            }

            UpdateAvatar();

            Selection.activeObject = m_selectedAvatar;
        }

        private void UpdateAvatar()
        {
            if (m_selectedAvatar != null)
            {
                Animator animator = m_selectedAvatar.GetComponent<Animator>();

                if (animator != null)
                {
                    var avatarControl = m_selectedAvatar.GetComponent<Avatar>();

                    if (avatarControl)
                    {
                        // HMD alignment
                        Transform leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
                        Transform rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
                        Vector3 avgEyeVec;

                        if (leftEye != null && rightEye != null)
                        {
                            avgEyeVec = (leftEye.position + rightEye.position) / 2.0f;
                            avatarControl.m_targetHMD.position = avgEyeVec + avatarControl.transform.forward * 0.2f;
                        }
                        else
                        {
                            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                            avatarControl.m_targetHMD.position = head.position + avatarControl.transform.forward * 0.2f;
                        }

                        // Left STEM alignment
                        Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        Transform leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        if (leftHand != null)
                        {
                            avatarControl.m_targetL.rotation = Quaternion.LookRotation(leftHand.position - leftElbow.position, avatarControl.transform.forward);
                            avatarControl.m_targetL.position = leftHand.position + (leftHand.position - leftElbow.position);
                        }

                        // Right STEM alignment
                        Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        Transform rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        if (rightHand != null)
                        {
                            avatarControl.m_targetR.rotation = Quaternion.LookRotation(rightHand.position - rightElbow.position, avatarControl.transform.forward);
                            avatarControl.m_targetR.position = rightHand.position + (rightHand.position - rightElbow.position);
                        }
                    }
                }
            }
        }
        #endregion

        #region GUI Presentation
        void UpdateGUI()
        {
            GUILayout.BeginVertical();
            {
                if (m_modelSelect || m_selectedAvatar == null)
                {
                    ModelSelect();
                }
                else
                {
                    GUILayout.Label("Edit Avatar Definition", EditorStyles.boldLabel);

                    if (GUILayout.Button("Change Model"))
                        m_modelSelect = true;

                    var avatarControl = m_selectedAvatar.GetComponent<Avatar>();

                    if (avatarControl != null)
                    {
                        m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

                        GUILayout.Label("Align Trackers", EditorStyles.boldLabel);

                        GUILayout.Label("In order to animate the avatar correctly, the tracking hardware must be manually lined up with the model.", EditorStyles.wordWrappedLabel);
                        GUILayout.Label("Click the buttons below to select each tracker, then position and orient them as if the avatar were holding the controllers in its hands, and wearing the HMD on its face.", EditorStyles.wordWrappedLabel);

                        GUILayout.Space(10);

                        if (GUILayout.Button("Head Mounted Display", GUILayout.Height(24)))
                        {
                            Selection.activeObject = avatarControl.m_targetHMD;
                            SceneView.lastActiveSceneView.FrameSelected();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Left Controller", GUILayout.Height(24)))
                        {
                            Selection.activeObject = avatarControl.m_targetL;
                            SceneView.lastActiveSceneView.FrameSelected();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Right Controller", GUILayout.Height(24)))
                        {
                            Selection.activeObject = avatarControl.m_targetR;
                            SceneView.lastActiveSceneView.FrameSelected();
                            SceneView.RepaintAll();
                        }

                        m_mirrorEnabled = GUILayout.Toggle(m_mirrorEnabled, "Mirror Left and Right");

                        GUILayout.FlexibleSpace();
                        GUILayout.EndScrollView();
                    }

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Reset Trackers"))
                        {
                            UpdateAvatar();
                        }
                        if (GUILayout.Button("Done"))
                        {
                            Close();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void ModelSelect()
        {
            GUILayout.Label("Model Selection", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh List"))
            {
                m_bInitialized = false;
            }
            GUILayout.Label("Only models that have rig animation type set to 'Humanoid' will appear in this list.", EditorStyles.wordWrappedLabel);

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, true);
            if (m_bInitialized)
            {
                int yPos = 300;
                int yStep = 100;

                int rowCount = 1;
                for (int i = 0; i < AvatarObjects.Count; i++)
                {
                    if (rowCount == 1)
                    {
                        GUILayout.BeginHorizontal();
                    }

                    GUI.backgroundColor = m_selectedModel == AvatarObjects[i] ? Color.magenta : Color.clear;

                    if (GUILayout.Button(AssetPreview.GetAssetPreview(AvatarObjects[i]), GUILayout.Width(128), GUILayout.Height(128)))
                    {
                        m_selectedModel = AvatarObjects[i];
                        Selection.activeObject = m_selectedModel;
                    }

                    yPos += yStep;

                    rowCount++;

                    if (128 * rowCount > position.width - 30 || i == AvatarObjects.Count - 1)
                    {
                        GUILayout.EndHorizontal();
                        rowCount = 1;
                    }
                }

                GUI.backgroundColor = Color.white;

                if (m_selectedModel == null && AvatarObjects.Count > 0)
                {
                    m_selectedModel = AvatarObjects[AvatarObjects.Count - 1];
                }
            }
            else
            {
                GUILayout.Space(30);
                GUILayout.Label("Loading...");

                m_doRefresh++;  // we need to wait a few frames before blocking to find assets
                Repaint();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Cancel"))
                {
                    if (m_selectedAvatar == null)
                        Close();

                    m_modelSelect = false;
                }

                if (m_selectedAvatar != null)
                {
                    if (GUILayout.Button("Select"))
                    {
                        SetModel();

                        m_modelSelect = false;
                    }
                }
                else if (GUILayout.Button("Create"))
                {
                    NewAvatar();

                    m_modelSelect = false;
                }
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region EditorWindow Events
        void OnGUI()
        {
            if (!ms_hasVersion)
            {
                version = "SixenseVR API " + SixenseVR.Device.APIVersion;

                ms_hasVersion = true;
            }

            if (m_doRefresh > 5)
            {
                m_bInitialized = true;
                m_doRefresh = 0;
                GetCompatibleHumanoids();
            }

            UpdateGUI();

            EditorGUILayout.LabelField(version);
        }

        // Update is called once per frame
        void Update()
        {
            if (m_mirrorRoot == null || EditorApplication.isPlaying || !m_mirrorEnabled)
                return;

            Transform current, other;

            if (Selection.activeGameObject == m_mirrorFirst.gameObject)
            {
                current = m_mirrorFirst;
                other = m_mirrorSecond;
            }
            else if (Selection.activeGameObject == m_mirrorSecond.gameObject)
            {
                current = m_mirrorSecond;
                other = m_mirrorFirst;
            }
            else return;

            Quaternion r = Quaternion.Inverse(m_mirrorRoot.rotation) * current.rotation;
            Vector3 p = m_mirrorRoot.InverseTransformPoint(current.position);

            p.x = -p.x;

            r.y = -r.y;
            r.z = -r.z;

            other.position = m_mirrorRoot.TransformPoint(p);
            other.rotation = m_mirrorRoot.rotation * r;
        }
        #endregion

        #region Entry Point
        [MenuItem("GameObject/Create Other/SixenseVR Avatar...")]
        public static CreateAvatarWindow Init()
        {
            // Get existing open window or if none, make a new one:
            CreateAvatarWindow window = (CreateAvatarWindow)EditorWindow.GetWindow(typeof(CreateAvatarWindow));
            window.title = "VR Avatar";

            return window;
        }
        #endregion
    }
}
