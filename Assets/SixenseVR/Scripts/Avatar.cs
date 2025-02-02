﻿//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEngine;
using System.Collections;

namespace SixenseVR
{
    [SelectionBase]
    public class Avatar : MonoBehaviour
    {
        #region Types
        [System.Serializable]
        public class AnimationVariables
        {
            [Tooltip("This variable will be set with the forward movement rate in meters/second.")]
            public string ForwardSpeed = "Speed";

            [Tooltip("This variable will be set with the rightward movement rate in meters/second.")]
            public string RightSpeed = "Strafe";

            [Tooltip("This variable will be set with the rightward turn rate in radians/second.")]
            public string RightTurnSpeed = "Direction";

            [Tooltip("This variable will range between 0 when the left hand is open and 1 when it is closed.")]
            public string HandCloseLeft = "HandCloseLeft";

            [Tooltip("This variable will range between 0 when the right hand is open and 1 when it is closed.")]
            public string HandCloseRight = "HandCloseRight";

            [Tooltip("The index of the animation controller layer for the left hand.")]
            public int LeftHandLayer = 2;

            [Tooltip("The index of the animation controller layer for the right hand.")]
            public int RightHandLayer = 3;
        }
        #endregion

        #region Public Members
        [Tooltip("Names of animation controller variables used for locomotion and grabbing.")]
        public AnimationVariables m_animationVariables;

        [Tooltip("If true, the body locomotion will be animation driven, while the head will move smoothly.  Not fully implemented.")]
        public bool m_rootMotion = false;

        [Tooltip("Joystick walking speed, in meters/second.")]
        public float m_walkSpeed = 1;
        [Tooltip("Joystick turning speed, in radians/second.")]
        public float m_turnSpeed = 2;

        [Tooltip("Use controller thumbsticks to move through the world.")]
        public bool m_joystickLocomotion = true;

        [Tooltip("Transform that defines the center of the safe area, all movement will be relative to it.")]
        public Transform m_safeAreaFollowsTransform;

        [Tooltip("If the configured safe area is less than this radius, joystick locomotion will be used.")]
        public float m_minimumSafeAreaRadius = 1;

        [Tooltip("List of accessories that will be automatically picked up by the Avatar, if extra STEM Packs are present.")]
        public Accessory[] m_autoGrabAccessories;

        public FixedBase m_fixedBase = null;
        #endregion

        #region Prefab Transform Hooks
        public Transform m_targetR;
        public Transform m_targetL;
        public Transform m_targetH;
        public Transform m_targetHMD;
        public Transform m_targetB;
        public Transform m_boundary;

        public Transform m_targetP1;
        public Transform m_targetP2;
        public Transform m_targetP3;
        public Transform m_targetP4;

        public Transform m_cameraMount;
        public Transform m_trackingForward;
        public TextMesh m_status;
        #endregion

        #region Private Members
        BodySolver m_solver;

        Animator m_anim;

        CharacterController m_character;

        Vector3[] m_jointPositions = new Vector3[(int)Joint.COUNT];

        Transform[] m_joints = new Transform[(int)Joint.COUNT];
        Quaternion[] m_bindPoses = new Quaternion[(int)Joint.COUNT];
        Camera[] m_cameras;

        Vector3[] m_trackerPositions = new Vector3[(int)Tracker.COUNT];
        Quaternion[] m_trackerRotations = new Quaternion[(int)Tracker.COUNT];

        BaseControl m_base;

        HandControl m_handL;
        HandControl m_handR;

        PackControl m_packH;

        PackControl m_packE1;
        PackControl m_packE2;
        PackControl m_packE3;
        PackControl m_packE4;

        BoundaryFence m_fence;

        bool m_hasBody = false;

        float m_angle = 0;

        bool m_hasTracking = false;
        bool m_hasSkeleton = false;

        bool m_constrained = false;
        Vector3 m_safeAreaCenter;
        Quaternion m_safeAreaRotation;

        bool m_needsTeleport = true;

        Vector3 m_velocity = Vector3.zero;
        Vector3 m_moveDelta = Vector3.zero;
        bool m_debugShow = false;

        float m_visTimer = 0;

        SixenseCore.Tracker[] m_inputs = new SixenseCore.Tracker[(int)Tracker.COUNT];

        System.Reflection.MethodInfo m_dismiss = null;
        #endregion

        #region Accessors
        public BodySolver Solver
        {
            get
            {
                return m_solver;
            }
        }
        #endregion

        #region Initialization
        void SetupArm(Joint shoulder, Joint elbow, Joint wrist, HandControl hand)
        {
            {
                var bone = m_joints[(int)shoulder];

                Vector3 pivot = bone.InverseTransformDirection(
                    Vector3.Cross(m_joints[(int)elbow].position - bone.position,
                                    transform.forward)).normalized;

                Vector3 dir = bone.InverseTransformDirection(
                    m_joints[(int)elbow].position - bone.position).normalized;

                m_bindPoses[(int)shoulder] = Quaternion.Inverse(Quaternion.LookRotation(dir, pivot));
            }
            {
                var bone = m_joints[(int)elbow];

                Vector3 pivot = bone.InverseTransformDirection(
                    Vector3.Cross(m_joints[(int)wrist].position - bone.position,
                                    transform.forward)).normalized;

                Vector3 dir = bone.InverseTransformDirection(
                    m_joints[(int)wrist].position - bone.position).normalized;

                m_bindPoses[(int)elbow] = Quaternion.Inverse(Quaternion.LookRotation(dir, pivot));
            }
            {
                var bone = m_joints[(int)wrist];

                m_bindPoses[(int)wrist] = Quaternion.Inverse(hand.m_sensor.rotation) * bone.rotation;

                Vector3 offset = hand.m_sensor.InverseTransformPoint(bone.position) * 1000;

                hand.m_bone = bone;

                m_solver.SetJointOffset((Tracker)hand.m_hand, offset);
            }
        }
        void SetupLeg(Joint shoulder, Joint elbow, Joint wrist)
        {
            {
                var bone = m_joints[(int)shoulder];

                Vector3 pivot = bone.InverseTransformDirection(
                    Vector3.Cross(m_joints[(int)elbow].position - bone.position,
                                    transform.forward)).normalized;

                Vector3 dir = bone.InverseTransformDirection(
                    m_joints[(int)elbow].position - bone.position).normalized;

                m_bindPoses[(int)shoulder] = Quaternion.Inverse(Quaternion.LookRotation(dir, pivot));
            }
            {
                var bone = m_joints[(int)elbow];

                Vector3 pivot = bone.InverseTransformDirection(
                    Vector3.Cross(m_joints[(int)wrist].position - bone.position,
                                    transform.forward)).normalized;

                Vector3 dir = bone.InverseTransformDirection(
                    m_joints[(int)wrist].position - bone.position).normalized;

                m_bindPoses[(int)elbow] = Quaternion.Inverse(Quaternion.LookRotation(dir, pivot));
            }
            {
                var bone = m_joints[(int)wrist];

                m_bindPoses[(int)wrist] = Quaternion.Inverse(transform.rotation) * bone.rotation;
            }
        }

        void SetupIK()
        {
            if (m_hasBody)
            {
                // uses mecanim humanoid bindings
                for (var j = (Joint)0; j < Joint.COUNT; j++)
                {
                    HumanBodyBones bone = BodySolver.ToUnityBone(j);
                    m_joints[(int)j] = m_anim.GetBoneTransform(bone);
                }

                if (m_joints[(int)Joint.NECK] == null)  // if the rig does not have a valid neck bone, just use the head
                    m_joints[(int)Joint.NECK] = m_anim.GetBoneTransform(HumanBodyBones.Head);

                m_joints[(int)Joint.WRIST_RIGHT].gameObject.AddComponent<Rigidbody>().isKinematic = true;
                m_joints[(int)Joint.WRIST_LEFT].gameObject.AddComponent<Rigidbody>().isKinematic = true;

                SetupArm(Joint.SHOULDER_LEFT, Joint.ELBOW_LEFT, Joint.WRIST_LEFT, m_handL);
                SetupArm(Joint.SHOULDER_RIGHT, Joint.ELBOW_RIGHT, Joint.WRIST_RIGHT, m_handR);

                SetupLeg(Joint.HIP_LEFT, Joint.KNEE_LEFT, Joint.ANKLE_LEFT);
                SetupLeg(Joint.HIP_RIGHT, Joint.KNEE_RIGHT, Joint.ANKLE_RIGHT);

                {
                    var j = Joint.NECK;
                    var bone = m_joints[(int)j];

                    m_bindPoses[(int)j] = Quaternion.Inverse(m_targetHMD.rotation) * bone.rotation;

                    Vector3 offset = m_targetHMD.InverseTransformPoint(bone.position);
                    m_solver.SetJointOffset(Tracker.HEAD, offset);
                }
            }
        }
        #endregion

        #region Input
        public SixenseCore.Tracker GetInput(Tracker t)
        {
            var i = m_inputs[(int)t];

            if (!i.Enabled)
                return null;

            return i;
        }

        void HandleInput()
        {
            // get latest tracker data
            for (int i = 0; i < (int)Tracker.COUNT; i++)
            {
                m_solver.UpdateTrackerInput((Tracker)i, ref m_inputs[i]);

                // for Oculus, dismiss health and safety warning if a button is pressed
                if (m_inputs[i].GetAnyButtonDown())
                {
                    if (m_dismiss != null)
                        m_dismiss.Invoke(null, null);

                    m_dismiss = null;
                }
            }

            var left = GetInput(Tracker.LEFT);
            var right = GetInput(Tracker.RIGHT);
            var head = GetInput(Tracker.HEAD);
            var p1 = GetInput(Tracker.EXTRA_1);
            var p2 = GetInput(Tracker.EXTRA_2);
            var p3 = GetInput(Tracker.EXTRA_3);
            var p4 = GetInput(Tracker.EXTRA_4);

            m_handL.m_controller = left;
            m_handR.m_controller = right;

            m_constrained = m_solver.BaseConstrained;

            // show active devices
            {
                if (left == null)
                {
                    if (m_handL.gameObject.activeSelf)
                    {
                        m_handL.Drop();
                    }
                    m_handL.gameObject.SetActive(false);
                }
                else
                {
                    m_handL.gameObject.SetActive(true);
                }
                if (right == null)
                {
                    if (m_handR.gameObject.activeSelf)
                    {
                        m_handR.Drop();
                    }
                    m_handR.gameObject.SetActive(false);
                }
                else
                {
                    m_handR.gameObject.SetActive(true);
                }

                m_packH.gameObject.SetActive(head != null);
                m_packE1.gameObject.SetActive(p1 != null);
                m_packE2.gameObject.SetActive(p2 != null);
                m_packE3.gameObject.SetActive(p3 != null);
                m_packE4.gameObject.SetActive(p4 != null);

                m_base.gameObject.SetActive(m_hasTracking);
            }

            Vector3 desiredMotion = Vector3.zero;

            bool baseVis = false;

            SixenseCore.Tracker moveController = null;
            SixenseCore.Tracker steerController = null;

            // bind controllers to locomotion
            if (left != null)
            {
                moveController = left;
                steerController = right;
            }
            else if (right != null)
            {
                moveController = right;
            }

            // grasping
            if (left != null)
            {
                m_anim.SetFloat(m_animationVariables.HandCloseLeft, left.Trigger, 0.025f, Time.deltaTime);
            }
            if (right != null)
            {
                m_anim.SetFloat(m_animationVariables.HandCloseRight, right.Trigger, 0.025f, Time.deltaTime);
            }

            // keyboard input
            if (m_joystickLocomotion || !m_constrained)
            {
                float turn = 0;

                if (Input.GetKey(KeyCode.Q))
                    turn = -1.0f;
                if (Input.GetKey(KeyCode.E))
                    turn = 1.0f;

                m_cameraMount.rotation *= Quaternion.AngleAxis(turn, Vector3.up);
                m_trackingForward.rotation *= Quaternion.AngleAxis(turn, Vector3.up);
                m_angle += turn * m_turnSpeed;

                Vector3 stick = Vector3.zero;

                if (Input.GetKey(KeyCode.A))
                    stick.x = -1.0f;
                if (Input.GetKey(KeyCode.D))
                    stick.x = 1.0f;
                if (Input.GetKey(KeyCode.S))
                    stick.z = -1.0f;
                if (Input.GetKey(KeyCode.W))
                    stick.z = 1.0f;

                Quaternion moveRelative = Quaternion.Euler(0, m_cameras[1].transform.rotation.eulerAngles.y, 0);
                desiredMotion = moveRelative * stick * m_walkSpeed;
            }

            // jotstick input
            if (m_joystickLocomotion || !m_constrained)
            {
                if (steerController != null)
                {
                    float turn = steerController.JoystickX;

                    m_cameraMount.rotation *= Quaternion.AngleAxis(turn * 2.0f, Vector3.up);
                    m_trackingForward.rotation *= Quaternion.AngleAxis(turn * 2.0f, Vector3.up);
                    m_angle += turn * m_turnSpeed;
                }

                if (moveController != null)
                {
                    Vector3 stick = new Vector3(moveController.JoystickX, 0, moveController.JoystickY);

                    Quaternion moveRelative = Quaternion.Euler(0, m_cameras[1].transform.rotation.eulerAngles.y, 0);

                    if (stick.sqrMagnitude > 0)
                    {
                        desiredMotion = moveRelative * stick * m_walkSpeed;

                        Vector3 heading = m_cameras[1].transform.forward;
                        heading.y = 0;
                        heading = heading.normalized;

                        Quaternion moveDir = Quaternion.LookRotation(
                            Vector3.Dot(desiredMotion.normalized, heading) >= -0.2f ? desiredMotion : -desiredMotion,
                            Vector3.up);

                        m_angle = moveDir.eulerAngles.y;
                    }
                }
            }

            // torso direction
            if (m_hasTracking && left != null && right != null)
            {
                Quaternion torso = m_solver.TorsoRotation;

                m_angle = torso.eulerAngles.y;
            }
            else if (Mathf.Abs(Mathf.DeltaAngle(m_solver.TorsoRotation.eulerAngles.y, m_angle)) > 45)
            {
                m_angle = m_solver.TorsoRotation.eulerAngles.y;
            }

            if (Input.GetKey(KeyCode.Backslash))
                m_debugShow = !m_debugShow;

            // Visualize hardware if it is close to the base
            {
                float dist = UserProfile.VisibleBaseDistance;

                if (head == null)
                    baseVis = false;
                else if (
                        (left == null || !left.Enabled || left.Position.sqrMagnitude > dist * dist) &&
                        (right == null || !right.Enabled || right.Position.sqrMagnitude > dist * dist) &&
                        (head.Position.sqrMagnitude > dist * dist))
                    baseVis = false;
                else
                    baseVis = true;

                if (baseVis)
                    m_visTimer = 2;
                else if (m_visTimer > 0)
                    m_visTimer -= Time.deltaTime;

                if ((left != null && left.GetButton(SixenseCore.Buttons.START)) ||
                    (right != null && right.GetButton(SixenseCore.Buttons.START)))
                    m_visTimer = 5;

                if (m_debugShow || m_visTimer > 0)
                    foreach (var c in m_cameras)
                    {
                        if (c.orthographic)
                            continue;
                        c.cullingMask |= (1 << m_targetB.gameObject.layer);
                    }
                else
                    foreach (var c in m_cameras)
                    {
                        if (c.orthographic)
                            continue;
                        c.cullingMask &= ~(1 << m_targetB.gameObject.layer);
                    }

                m_boundary.gameObject.SetActive(m_visTimer <= 0);
            }

            // set animation parameters
            {
                m_anim.SetFloat(m_animationVariables.ForwardSpeed, Vector3.Dot(desiredMotion, transform.forward), 0.5f, Time.deltaTime);
                m_anim.SetFloat(m_animationVariables.RightSpeed, Vector3.Dot(desiredMotion, transform.right), 0.5f, Time.deltaTime);

                m_anim.SetFloat(m_animationVariables.RightTurnSpeed, Mathf.DeltaAngle(transform.rotation.eulerAngles.y, m_angle) * 2f, 0.01f, Time.deltaTime);

                if (!m_rootMotion)
                {
                    m_velocity = Vector3.Lerp(m_velocity, desiredMotion * 1.2f, Time.deltaTime * 5f);
                }
            }
        }
        #endregion

        #region Tracking
        void UpdateTracking()
        {
            if (m_needsTeleport)
                m_solver.HandleTeleport();

            m_needsTeleport = false;

            // handle fixed safe area
            if (m_safeAreaFollowsTransform != null)
            {
                m_safeAreaCenter = m_safeAreaFollowsTransform.position;
                m_safeAreaRotation = m_safeAreaFollowsTransform.rotation;
            }

            if (m_minimumSafeAreaRadius <= SixenseVR.UserProfile.SafeAreaRadius + Mathf.Epsilon &&
                !m_joystickLocomotion && m_fixedBase == null)
            {
                Vector3 center = SixenseVR.UserProfile.SafeAreaCenter;

                Vector3 basepos = m_safeAreaRotation * new Vector3(-center.x, 0, -center.z) + m_safeAreaCenter;

                m_solver.ConstrainBaseTransform(basepos, m_safeAreaRotation, false);
            }

            {
                m_targetHMD.position = (m_cameras[m_cameras.Length - 1].transform.position + m_cameras[m_cameras.Length - 2].transform.position) * 0.5f;
                m_targetHMD.rotation = m_cameras[1].transform.rotation;
            }
            {
                // feed SixenseVR all the necessary per-frame info and compute skeleton solution
                m_solver.UpdateEyeTransform(m_targetHMD.position, m_targetHMD.rotation, false);

                m_solver.UpdateOrigin(transform.position, m_trackingForward.rotation);
            }

            m_hasTracking = m_solver.SolveTracking();

            for (int i = 0; i < (int)Tracker.COUNT; i++)
            {
                Vector3 p;
                Quaternion q;
                m_solver.GetTrackerTransform((Tracker)i, out p, out q);

                if (IsValid(p))
                    m_trackerPositions[i] = p;
                if (IsValid(q))
                    m_trackerRotations[i] = q;
            }

            {
                // get out where SixenseVR thinks the trackers are in world space

                if (m_hasTracking)
                {
                    m_targetB.rotation = m_trackerRotations[(int)Tracker.BASE] * Quaternion.Inverse(m_base.m_emitter.localRotation);
                    m_targetB.position += m_trackerPositions[(int)Tracker.BASE] - m_base.m_emitter.position;

                    m_targetH.rotation = m_trackerRotations[(int)Tracker.HEAD] * Quaternion.Inverse(m_packH.m_sensor.localRotation);
                    m_targetH.position += m_trackerPositions[(int)Tracker.HEAD] - m_packH.m_sensor.position;

                    m_targetR.rotation = m_trackerRotations[(int)Tracker.RIGHT] * Quaternion.Inverse(m_handR.m_sensor.localRotation);
                    m_targetR.position += m_trackerPositions[(int)Tracker.RIGHT] - m_handR.m_sensor.position;

                    m_targetL.rotation = m_trackerRotations[(int)Tracker.LEFT] * Quaternion.Inverse(m_handL.m_sensor.localRotation);
                    m_targetL.position += m_trackerPositions[(int)Tracker.LEFT] - m_handL.m_sensor.position;
               
                    m_targetP1.rotation = m_trackerRotations[(int)Tracker.EXTRA_1] * Quaternion.Inverse(m_packE1.m_sensor.localRotation);
                    m_targetP1.position += m_trackerPositions[(int)Tracker.EXTRA_1] - m_packE1.m_sensor.position;

                    m_targetP2.rotation = m_trackerRotations[(int)Tracker.EXTRA_2] * Quaternion.Inverse(m_packE2.m_sensor.localRotation);
                    m_targetP2.position += m_trackerPositions[(int)Tracker.EXTRA_2] - m_packE2.m_sensor.position;

                    m_targetP3.rotation = m_trackerRotations[(int)Tracker.EXTRA_3] * Quaternion.Inverse(m_packE3.m_sensor.localRotation);
                    m_targetP3.position += m_trackerPositions[(int)Tracker.EXTRA_3] - m_packE3.m_sensor.position;

                    m_targetP4.rotation = m_trackerRotations[(int)Tracker.EXTRA_4] * Quaternion.Inverse(m_packE4.m_sensor.localRotation);
                    m_targetP4.position += m_trackerPositions[(int)Tracker.EXTRA_4] - m_packE4.m_sensor.position;

                    m_boundary.rotation = m_trackerRotations[(int)Tracker.BASE];
                    m_boundary.position = m_trackerPositions[(int)Tracker.BASE];
                }
            }
        }

        void UpdateEyes()
        {
            Vector3 newEyePos;
            Quaternion newEyeRot;
            m_solver.GetEyeTransform(out newEyePos, out newEyeRot);

            if (m_hasTracking)
            {
                Quaternion diff = newEyeRot * Quaternion.Inverse(m_cameras[1].transform.rotation);
                diff.x = 0; // dont allow pitch or roll
                diff.z = 0;

                m_cameraMount.rotation *= diff;
            }

            Vector3 eyeDelta = newEyePos - (m_cameras[m_cameras.Length - 1].transform.position + m_cameras[m_cameras.Length - 2].transform.position) * 0.5f;

            m_cameras[m_cameras.Length - 1].transform.position += eyeDelta;
            m_cameras[m_cameras.Length - 2].transform.position += eyeDelta;

            m_targetHMD.position = newEyePos;
            m_targetHMD.rotation = newEyeRot;
        }
        #endregion

        #region Inverse Kinematics
        private static bool IsValid(Quaternion q)
        {
            if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w))
                return false;
            else if (float.IsInfinity(q.x) || float.IsInfinity(q.y) || float.IsInfinity(q.z) || float.IsInfinity(q.w))
                return false;

            return true;
        }
        private static bool IsValid(Vector3 v)
        {
            if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z))
                return false;
            else if (float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z))
                return false;

            return true;
        }

        void AnimateArm(Joint root, Joint mid, Joint end, JointChain chain)
        {
            Vector3 shoulderPos = m_jointPositions[(int)root];
            Vector3 elbowPos = m_jointPositions[(int)mid];
            Vector3 wristPos = m_jointPositions[(int)end];

            if (Application.isEditor)
            {
                Debug.DrawLine(shoulderPos, elbowPos);
                Debug.DrawLine(elbowPos, wristPos);
            }

            Vector3 shoulderToElbow = elbowPos - shoulderPos;
            Vector3 elbowToWeapon = wristPos - elbowPos;

            Vector3 fwd = shoulderToElbow.normalized;
            Vector3 left = m_solver.GetHingeAxis(chain);

            Quaternion shoulderRot = Quaternion.LookRotation(fwd, left);

            fwd = elbowToWeapon.normalized;

            Quaternion elbowRot = Quaternion.LookRotation(fwd, left);

            // set transforms
            var wristRot = m_joints[(int)end].rotation;
            m_joints[(int)root].position = shoulderPos;
            m_joints[(int)root].rotation = shoulderRot * m_bindPoses[(int)root];
            m_joints[(int)mid].rotation = elbowRot * m_bindPoses[(int)mid];
            m_joints[(int)end].rotation = wristRot;
        }
        void AnimateLeg(Joint root, Joint mid, Joint end, JointChain chain)
        {
            Vector3 shoulderPos = m_jointPositions[(int)root];
            Vector3 elbowPos = m_jointPositions[(int)mid];
            Vector3 wristPos = m_jointPositions[(int)end];

            if (Application.isEditor)
            {
                Debug.DrawLine(shoulderPos, elbowPos);
                Debug.DrawLine(elbowPos, wristPos);
            }

            Vector3 shoulderToElbow = elbowPos - shoulderPos;
            Vector3 elbowToWeapon = wristPos - elbowPos;

            Vector3 fwd = shoulderToElbow.normalized;
            Vector3 left = m_solver.GetHingeAxis(chain);

            Quaternion shoulderRot = Quaternion.LookRotation(fwd, left);

            fwd = elbowToWeapon.normalized;

            Quaternion elbowRot = Quaternion.LookRotation(fwd, left);

            // set transforms
            Quaternion footRot = m_joints[(int)end].rotation;
            m_joints[(int)root].position = shoulderPos;
            m_joints[(int)root].rotation = shoulderRot * m_bindPoses[(int)root];
            m_joints[(int)mid].rotation = elbowRot * m_bindPoses[(int)mid];
            m_joints[(int)end].rotation = footRot;
        }

        void ApplyTargetRotation(Quaternion rot, Joint joint, float weight = 1.0f)
        {
            var q = Quaternion.Slerp(m_joints[(int)joint].rotation, rot * m_bindPoses[(int)joint], Mathf.Clamp01(weight));

            if (IsValid(q))
                m_joints[(int)joint].rotation = q;
        }

        void AnimateIK()
        {
            if (m_hasBody)
            {
                for (var j = (Joint)0; j < Joint.COUNT; j++)
                {
                    m_jointPositions[(int)j] = m_joints[(int)j].position;
                }

                m_solver.JointPositions = m_jointPositions;

                // constrain joints
                {
                    var j = Joint.ELBOW_LEFT;
                    var c = JointChain.ARM_LEFT;
                    Vector3 left = m_joints[(int)j].TransformDirection(Quaternion.Inverse(m_bindPoses[(int)j]) * Vector3.up);
                    m_solver.UpdateHingeAxis(c, left);
                }
                {
                    var j = Joint.ELBOW_RIGHT;
                    var c = JointChain.ARM_RIGHT;
                    Vector3 left = m_joints[(int)j].TransformDirection(Quaternion.Inverse(m_bindPoses[(int)j]) * Vector3.up);
                    m_solver.UpdateHingeAxis(c, left);
                }
                {
                    var j = Joint.KNEE_LEFT;
                    var c = JointChain.LEG_LEFT;
                    Vector3 left = m_joints[(int)j].TransformDirection(Quaternion.Inverse(m_bindPoses[(int)j]) * Vector3.up);
                    m_solver.UpdateHingeAxis(c, left);
                }
                {
                    var j = Joint.KNEE_RIGHT;
                    var c = JointChain.LEG_RIGHT;
                    Vector3 left = m_joints[(int)j].TransformDirection(Quaternion.Inverse(m_bindPoses[(int)j]) * Vector3.up);
                    m_solver.UpdateHingeAxis(c, left);
                }

                m_hasSkeleton = m_solver.SolveSkeleton();

                m_jointPositions = m_solver.JointPositions;

                if (m_hasBody && m_hasSkeleton)
                for (var j = (Joint)0; j < Joint.COUNT; j++)
                {
                    m_joints[(int)j].position = m_jointPositions[(int)j];
                }

                m_moveDelta = m_solver.MoveDelta * 0.25f;
            }
            
            if (!m_hasBody || !m_hasSkeleton)
            {
                return; // nothing to animate
            }
            {
                AnimateArm(
                    Joint.SHOULDER_RIGHT,
                    Joint.ELBOW_RIGHT,
                    Joint.WRIST_RIGHT,
                    JointChain.ARM_RIGHT);

                float weight = m_solver.GetChainBlendWeight(JointChain.ARM_RIGHT);
                ApplyTargetRotation(m_handR.m_sensor.rotation, Joint.WRIST_RIGHT, weight);

                m_anim.SetLayerWeight(m_animationVariables.RightHandLayer, weight);
            }
            {
                AnimateArm(
                    Joint.SHOULDER_LEFT,
                    Joint.ELBOW_LEFT,
                    Joint.WRIST_LEFT,
                    JointChain.ARM_LEFT);

                float weight = m_solver.GetChainBlendWeight(JointChain.ARM_LEFT);
                ApplyTargetRotation(m_handL.m_sensor.rotation, Joint.WRIST_LEFT, weight);

                m_anim.SetLayerWeight(m_animationVariables.LeftHandLayer, weight);
            }
            {
                AnimateLeg(
                    Joint.HIP_RIGHT,
                    Joint.KNEE_RIGHT,
                    Joint.ANKLE_RIGHT,
                    JointChain.LEG_RIGHT);
            }
            {
                AnimateLeg(
                    Joint.HIP_LEFT,
                    Joint.KNEE_LEFT,
                    Joint.ANKLE_LEFT,
                    JointChain.LEG_LEFT);
            }
            {
                ApplyTargetRotation(m_targetHMD.rotation, Joint.NECK);
            }

            if (Application.isEditor)
            {
                Vector3 shoulderL = m_jointPositions[(int)Joint.SHOULDER_LEFT];
                Vector3 shoulderR = m_jointPositions[(int)Joint.SHOULDER_RIGHT];
                Vector3 spine = m_jointPositions[(int)Joint.SPINE];
                Vector3 neck = m_jointPositions[(int)Joint.NECK];

                Debug.DrawLine(shoulderR, shoulderL);
                Debug.DrawLine(shoulderR, spine);
                Debug.DrawLine(shoulderL, spine);
                Debug.DrawLine(shoulderR, neck);
                Debug.DrawLine(shoulderL, neck);
                Debug.DrawLine(neck, spine);

                Vector3 hipL = m_jointPositions[(int)Joint.HIP_LEFT];
                Vector3 hipR = m_jointPositions[(int)Joint.HIP_RIGHT];
                Vector3 root = m_jointPositions[(int)Joint.ROOT];

                Debug.DrawLine(hipL, hipR);
                Debug.DrawLine(hipR, root);
                Debug.DrawLine(hipL, root);
                Debug.DrawLine(spine, root);
            }
            {
                // if everything worked, this shouldn't be necessary, but for now highlight any rotation-position mismatches
                for (var j = (Joint)0; j < Joint.COUNT; j++)
                {
                    var p = m_jointPositions[(int)j];

                    if(IsValid(p))
                        m_joints[(int)j].position = p;
                }
            }
        }
        #endregion

        #region Accessories
        private void BindAccessory(Accessory a)
        {
            if (a == null)
                return;

            var pack = GetInput(a.m_pack);

            if (pack != null)
            {
                var body = a.gameObject.GetComponentInParent<Rigidbody>();

                foreach(var grab in body.gameObject.GetComponentsInChildren<GrabPoint>())
                {
                    var h = a.m_offhand ^ SixenseVR.UserProfile.Handedness == Hand.LEFT ? m_handL : m_handR;

                    if (grab.m_handedness == Hand.LEFT)
                        h = m_handL;
                    else if (grab.m_handedness == Hand.RIGHT)
                        h = m_handR;

                    Vector3 posoff = a.m_sensor.InverseTransformPoint(grab.m_sensor.position);

                    Quaternion rotoff = Quaternion.Inverse(a.m_sensor.rotation) * grab.m_sensor.rotation;

                    m_solver.SetHandAccessoryBindOffset((Hand)h.m_hand, a.m_pack, posoff, rotoff);

                    h.Drop();
                    h.Grab(body, grab, true);
                }

                a.Input = pack;
            }
        }
        #endregion

        #region Text Display
        private bool StatusDisplay(Tracker t)
        {
            var c = GetInput(t);

            if (c == null || !c.Connected)
                return false;

            string text = "";

            if (!c.Enabled)
            {
                if (c.HardwareType == SixenseCore.Hardware.STEM_CONTROLLER || c.HardwareType == SixenseCore.Hardware.HYDRA_CONTROLLER)
                    text = "Controller Must Be Re-docked";
                else if (c.HardwareType == SixenseCore.Hardware.STEM_PACK)
                    text = "Pack Must Be Re-docked";
            }
            else if (c.Docked)
            {
                if (c.HardwareType == SixenseCore.Hardware.STEM_CONTROLLER || c.HardwareType == SixenseCore.Hardware.HYDRA_CONTROLLER)
                    text = "Pick Up Controllers";
                else if (t == Tracker.HEAD)
                    text = "Attach Center Pack to HMD";
                else if (c.HardwareType == SixenseCore.Hardware.STEM_PACK)
                    text = "Pick Up And Attach Packs";
            }
            else if (c.BatteryLow && Mathf.FloorToInt(Time.time * 0.5f) % 10 == 0)
            {
                if (c.HardwareType == SixenseCore.Hardware.STEM_CONTROLLER || c.HardwareType == SixenseCore.Hardware.HYDRA_CONTROLLER)
                    text = t.ToString() + " Battery Low";
                else if (c.HardwareType == SixenseCore.Hardware.STEM_PACK)
                    text = t.ToString() + " Battery Low";
            }
            else
            {
                return false;
            }

            if (text != m_status.text)
                m_status.text = text;

            return true;
        }
        #endregion

        #region Editor Visualization
        void DrawCylinder(Vector3 position, float radius, float height)
        {
            Vector3 bottom = position;
            Vector3 top = bottom + Vector3.up * height;

            Gizmos.matrix = Matrix4x4.TRS(bottom, Quaternion.identity, new Vector3(radius, height, radius));

            Gizmos.DrawLine(Vector3.forward, Vector3.up + Vector3.forward);
            Gizmos.DrawLine(Vector3.back, Vector3.up + Vector3.back);
            Gizmos.DrawLine(Vector3.left, Vector3.up + Vector3.left);
            Gizmos.DrawLine(Vector3.right, Vector3.up + Vector3.right);

            Gizmos.matrix = Matrix4x4.TRS(bottom, Quaternion.identity, new Vector3(radius, 0, radius));

            Gizmos.DrawWireSphere(Vector3.zero, 1);

            Gizmos.matrix = Matrix4x4.TRS(top, Quaternion.identity, new Vector3(radius, 0, radius));

            Gizmos.DrawWireSphere(Vector3.zero, 1);
        }

        void OnDrawGizmos()
        {
            if (m_joystickLocomotion || Application.isPlaying)
                return;

            Gizmos.color = Color.yellow;

            var t = m_safeAreaFollowsTransform;
            if (t == null)
                t = transform;

            DrawCylinder(t.position, m_minimumSafeAreaRadius, 2);
        }
        #endregion

        #region Monobehaviour Events
        void Start()
        {
            // setup SixenseVR
            if(!Device.Init())
            {
                enabled = false;
                return;
            }

            m_solver = new BodySolver(true);
            m_solver.FreeBase();

            m_safeAreaCenter = transform.position;
            m_safeAreaRotation = transform.rotation;

            for (int i = 0; i < (int)Tracker.COUNT; i++)
                m_inputs[i] = new SixenseCore.Tracker(null);

            // get prefab components
            m_cameras = m_cameraMount.gameObject.GetComponentsInChildren<Camera>();

            m_base = m_targetB.gameObject.GetComponent<BaseControl>();

            m_handL = m_targetL.gameObject.GetComponent<HandControl>();
            m_handR = m_targetR.gameObject.GetComponent<HandControl>();

            m_packH = m_targetH.gameObject.GetComponent<PackControl>();

            m_packE1 = m_targetP1.gameObject.GetComponent<PackControl>();
            m_packE2 = m_targetP2.gameObject.GetComponent<PackControl>();
            m_packE3 = m_targetP3.gameObject.GetComponent<PackControl>();
            m_packE4 = m_targetP4.gameObject.GetComponent<PackControl>();

            m_fence = m_boundary.gameObject.GetComponent<BoundaryFence>();

            m_fence.m_avatar = this;
            m_fence.m_solver = m_solver;

            m_character = gameObject.GetComponent<CharacterController>();

            m_anim = gameObject.GetComponent<Animator>();

            m_handL.AvatarAnimator = m_anim;
            m_handR.AvatarAnimator = m_anim;

            // grab animation avatar from child mesh
            foreach (var a in gameObject.GetComponentsInChildren<Animator>())
            {
                if (a != m_anim && a.isHuman)
                {
                    m_anim.avatar = a.avatar;
                    if (a.runtimeAnimatorController != null)
                        m_anim.runtimeAnimatorController = a.runtimeAnimatorController;

                    Destroy(a);
                    m_hasBody = true;
                    break;
                }
            }

            // set hidden layer on cameras
            foreach (var c in m_cameras)
            {
                c.cullingMask &= ~(1 << m_targetH.gameObject.layer);
            }

            m_angle = m_cameraMount.rotation.eulerAngles.y;

            // use reflection to bind with OVR method to dismiss the health and safety warning with a button press
            { 
                System.Type ovr = System.Type.GetType("OVRManager");
                if (ovr != null)
                    m_dismiss = ovr.GetMethod("DismissHSWDisplay");
            }

            SetupIK();
        }

        void OnApplicationQuit()
        {
            Device.Shutdown();
        }

        void OnAnimatorMove()
        {
            if (m_rootMotion)   // not yet fully functional
            {
                m_character.Move(m_anim.deltaPosition + m_moveDelta + Physics.gravity * Time.deltaTime);
                m_moveDelta = Vector3.zero;
            }

            // rotate the root transform without rotating these children 
            Quaternion camr = m_cameraMount.rotation;
            Quaternion frontr = m_trackingForward.rotation;
            {
                transform.rotation *= m_anim.deltaRotation;
            }
            m_cameraMount.rotation = camr;
            m_trackingForward.rotation = frontr;
        }

        void Update()
        {
            // update text status display
            if(m_status != null)
            {
                bool en = false;
                if( StatusDisplay(Tracker.HEAD) ||
                    StatusDisplay(Tracker.RIGHT) ||
                    StatusDisplay(Tracker.LEFT) ||
                    StatusDisplay(Tracker.EXTRA_1) ||
                    StatusDisplay(Tracker.EXTRA_2))
                    en = true;

                if (en != m_status.GetComponent<Renderer>().enabled)
                    m_status.GetComponent<Renderer>().enabled = en;
            }

            // attach accessories when start button is pressed
            foreach (var a in m_autoGrabAccessories)
            {
                var pack = GetInput(a.m_pack);

                if(pack != null && pack.GetButtonDown(SixenseCore.Buttons.START))
                    BindAccessory(a);
            }

            HandleInput();
        }

        void FixedUpdate()
        {
            if (!m_rootMotion)  // use desired velocity and physical body displacement to update position
            {
                m_character.Move((m_velocity + Physics.gravity) * Time.deltaTime + m_moveDelta);
                m_moveDelta = Vector3.zero;
            }

            UpdateTracking();   // get tracking data to perform physics on held objects
        }

        void LateUpdate()
        {
            UpdateTracking();   // update again to minimize latency

            UpdateEyes();   // head tracking

            AnimateIK();    // compute final bone transforms for rendering
        }
        #endregion
    }
}
