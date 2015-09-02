//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace SixenseVR
{
    public class BodySolver
    {
        #region Properties
        /// <summary>
        /// Which body is this? The local user is always 0
        /// </summary>
        public int Index
        {
            get
            {
                return m_index;
            }
        }

        /// <summary>
        /// Call in case any method returns failure to get more detailed information
        /// </summary>
        public string LastError
        {
            get
            {
                SetIndex();

                int size = 256;
                var sb = new System.Text.StringBuilder(size);
                size = sb.Capacity;

                if (!Plugin.SixenseVR_LastError(ref size, sb) || size == 0)
                {
                    //return null;
                }

                return sb.ToString();
            }
        }
        #endregion

        #region Setup
        /// <summary>
        /// Create a new body.  If local is false, serialization should be used to synchronize with another body on a remote host
        /// </summary>
        public BodySolver(bool local = true)
        {
            if (!Device.Initialized)
            {
                throw new UnityException("SixenseVR Device not initialized");
            }

            if (local)
            {
                m_index = 0;
            }
            else
            {
                Plugin.Sixense_CreateBody(out m_index);
            }
        }

        /// <summary>
        /// Specify the offset from a skeletal joint on the avatar from the tracking hardware
        /// </summary>
        public void SetJointOffset(Tracker hand, Vector3 offset)
        {
            SetIndex();
            Plugin.Sixense_SetJointOffset(hand, PluginTypes.ToSixenseVector(offset));
        }
        #endregion

        #region Network Serialization
        /// <summary>
        /// Capture the full state of the body solver IK in a bit stream to send to remote hosts.  The developer must supply network transport and handle other avatar state such as animation
        /// </summary>
        public byte[] Serialize()
        {
            SetIndex();

            int size = 256;
            IntPtr ptr = Marshal.AllocHGlobal(size);

            if (!Plugin.Sixense_SerializeBody(ref size, ptr))
            {
                Debug.LogWarning(LastError);

                return null;
            }

            var buffer = new byte[size];

            Marshal.Copy(ptr, buffer, 0, size);

            Marshal.FreeHGlobal(ptr);

            return buffer;
        }

        /// <summary>
        /// Accept solver IK state from a remote host.  Should be called in place of SolveTracking() for non-local avatars.
        /// </summary>
        public bool Deserialize(byte[] blob)
        {
            SetIndex();

            int size = blob.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(blob, 0, ptr, size);

            bool ret = Plugin.Sixense_DeserializeBody(size, ptr);

            Marshal.FreeHGlobal(ptr);

            if (!ret)
                Debug.LogWarning(LastError);

            return ret;
        }
        #endregion

        #region Gameplay Events
        /// <summary>
        /// In place of a STEM Controller, other trackers may be used on handheld objects to animate the avatar's arms.  This will define an explicit offset from a given tracker to a given hand bone.
        /// </summary>
        public void SetHandAccessoryBindOffset(Hand hand, Tracker tracker, Vector3 offsetPos, Quaternion offsetRot)
        {
            SetIndex();
            Plugin.Sixense_SetHandAccessoryBindOffset(hand, tracker, PluginTypes.ToSixenseVector(offsetPos * 1000), PluginTypes.ToSixenseQuat(offsetRot));
        }

        /// <summary>
        /// Should be called in any situation where the avatar's position is discontinuous, such as teleporting from one point to another.
        /// </summary>
        public void HandleTeleport()
        {
            SetIndex();
            Plugin.Sixense_HandleTeleport();
        }
        #endregion

        #region Tracking Input Data
        public void UpdateEyeTransform(Vector3 position, Quaternion rotation, bool isPositionTracked)
        {
            SetIndex();
            Plugin.Sixense_UpdateEyeTransform(PluginTypes.ToSixenseVector(position), PluginTypes.ToSixenseQuat(rotation), isPositionTracked);
        }

        public void UpdateOrigin(Vector3 position, Quaternion rotation)
        {
            SetIndex();
            Plugin.Sixense_UpdateOriginTransform(PluginTypes.ToSixenseVector(position), PluginTypes.ToSixenseQuat(rotation));
        }

        public void UpdateHingeAxis(JointChain joint, Vector3 direction)
        {
            SetIndex();
            Plugin.Sixense_UpdateHingeAxis(joint, PluginTypes.ToSixenseVector(direction));
        }

        public void ConstrainBaseTransform(Vector3 position, Quaternion rotation, bool fixedHeight = false)
        {
            SetIndex();
            Plugin.Sixense_SetBaseConstraintTransform(PluginTypes.ToSixenseVector(position), PluginTypes.ToSixenseQuat(rotation), true, fixedHeight);
        }

        public void FreeBase()
        {
            SetIndex();
            Plugin.Sixense_SetBaseConstraintTransform(PluginTypes.ToSixenseVector(Vector3.zero), PluginTypes.ToSixenseQuat(Quaternion.identity), false, false);
        }

        public bool BaseConstrained
        {
            get
            {
                SetIndex();
                bool c;
                Plugin.Sixense_GetBaseConstrained(out c);
                return c;
            }
        }

        public void SetChainBlendWeight(JointChain chain, float w)
        {
            SetIndex();
            Plugin.Sixense_SetChainBlendWeight(chain, w);
        }
        #endregion

        #region Tracking Output Data
        public void GetTrackerTransform(Tracker hand, out Vector3 pos, out Quaternion rot)
        {
            SetIndex();
            PluginTypes.Vector v;
            PluginTypes.Quat q;
            Plugin.Sixense_GetTrackerTransform(hand, out v, out q);
            pos = PluginTypes.ToUnityVector(v);
            rot = PluginTypes.ToUnityQuat(q);
        }

        public void UpdateTrackerInput(Tracker tracker, ref SixenseCore.Tracker input)
        {
            SixenseCore.PluginTypes.TrackedDeviceData d;
            Plugin.Sixense_GetTrackerData(tracker, out d);

            input.PreUpdate();
            input.ProcessData(d);
        }
        #endregion

        #region IK Output Data
        public Vector3[] JointPositions
        {
            get
            {
                SetIndex();
                PluginTypes.Skeleton skel;
                Plugin.Sixense_GetJointPositions(out skel);

                Vector3[] joints = new Vector3[(int)Joint.COUNT];

                for (int i = 0; i < (int)Joint.COUNT; i++)
                {
                    joints[i] = PluginTypes.ToUnityVector(skel.joints[i]);
                }

                return joints;
            }
            set
            {
                SetIndex();
                PluginTypes.Skeleton skel;
                skel.joints = new PluginTypes.Vector[(int)Joint.COUNT];

                for (int i = 0; i < (int)Joint.COUNT; i++)
                {
                    skel.joints[i] = PluginTypes.ToSixenseVector(value[i]);
                }

                Plugin.Sixense_UpdateJointPositions(skel);
            }
        }

        public Vector3 GetHingeAxis(JointChain joint)
        {
            SetIndex();
            PluginTypes.Vector v;
            Plugin.Sixense_GetHingeAxis(joint, out v);
            return PluginTypes.ToUnityVector(v);
        }
        public float GetChainBlendWeight(JointChain chain)
        {
            SetIndex();
            float w;
            Plugin.Sixense_GetChainBlendWeight(chain, out w);
            return w;
        }

        public void GetEyeTransform(out Vector3 pos, out Quaternion rot)
        {
            SetIndex();
            PluginTypes.Vector v;
            PluginTypes.Quat q;
            Plugin.Sixense_GetEyeTransform(out v, out q);
            pos = PluginTypes.ToUnityVector(v);
            rot = PluginTypes.ToUnityQuat(q);
        }

        public Vector3 MoveDelta
        {
            get
            {
                SetIndex();
                PluginTypes.Vector v;
                Plugin.Sixense_GetMovementDelta(out v);
                return PluginTypes.ToUnityVector(v);
            }
        }

        public Quaternion TorsoRotation
        {
            get
            {
                SetIndex();
                PluginTypes.Quat q;
                Plugin.Sixense_GetTorsoRotation(out q);
                return PluginTypes.ToUnityQuat(q);
            }
        }
        #endregion

        #region Update Solutions
        public bool SolveTracking()
        {
            Profiler.BeginSample("SixenseVR SolveTracking");

            SetIndex();
            bool r = Plugin.Sixense_SolveTracking();

            Profiler.EndSample();
            return r;
        }

        public bool SolveSkeleton()
        {
            Profiler.BeginSample("SixenseVR SolveSkeleton");

            SetIndex();
            bool r = Plugin.Sixense_SolveSkeleton();

            Profiler.EndSample();
            return r;
        }
        #endregion

        #region Private Members
        static int ms_currentIndex = -1;

        int m_index = -1;

        SixenseCore.Tracker[] m_trackers = new SixenseCore.Tracker[(int)Tracker.COUNT];
        #endregion

        #region Public Methods
        public static HumanBodyBones ToUnityBone(Joint joint)
        {
            switch (joint)
            {
                case Joint.ROOT:
                    return HumanBodyBones.Hips;
                case Joint.SPINE:
                    return HumanBodyBones.Spine;
                case Joint.NECK:
                    return HumanBodyBones.Neck;
                case Joint.SHOULDER_LEFT:
                    return HumanBodyBones.LeftUpperArm;
                case Joint.ELBOW_LEFT:
                    return HumanBodyBones.LeftLowerArm;
                case Joint.WRIST_LEFT:
                    return HumanBodyBones.LeftHand;
                case Joint.SHOULDER_RIGHT:
                    return HumanBodyBones.RightUpperArm;
                case Joint.ELBOW_RIGHT:
                    return HumanBodyBones.RightLowerArm;
                case Joint.WRIST_RIGHT:
                    return HumanBodyBones.RightHand;
                case Joint.HIP_LEFT:
                    return HumanBodyBones.LeftUpperLeg;
                case Joint.KNEE_LEFT:
                    return HumanBodyBones.LeftLowerLeg;
                case Joint.ANKLE_LEFT:
                    return HumanBodyBones.LeftFoot;
                case Joint.HIP_RIGHT:
                    return HumanBodyBones.RightUpperLeg;
                case Joint.KNEE_RIGHT:
                    return HumanBodyBones.RightLowerLeg;
                case Joint.ANKLE_RIGHT:
                    return HumanBodyBones.RightFoot;
            }

            throw new System.Exception("Joint type " + joint.ToString() + " is not handled.");
        }
        #endregion

        #region Private Methods
        private void SetIndex()
        {
            if (m_index == -1)
            {
                throw new UnityException("SixenseVR Device not initialized");
            }
            if (m_index != ms_currentIndex)
            {
                Plugin.Sixense_SetActiveBody(m_index);

                ms_currentIndex = m_index;
            }
        }
        #endregion

        #region Native Interface
        private partial class Plugin
        {
            const string module = "SixenseVR";

            #region Lifecycle
            [DllImport(module)]
            public static extern bool Sixense_CreateBody(out int b);
            [DllImport(module)]
            public static extern bool Sixense_SetActiveBody(int b);

            [DllImport(module)]
            public static extern bool Sixense_SerializeBody(ref int size, IntPtr blob);
            [DllImport(module)]
            public static extern bool Sixense_DeserializeBody(int size, IntPtr blob);

            [DllImport(module)]
            public static extern bool SixenseVR_LastError(ref int size, System.Text.StringBuilder error);
            #endregion

            #region Tracking API
            [DllImport(module)]
            public static extern void Sixense_SetBaseConstraintTransform(PluginTypes.Vector pos, PluginTypes.Quat rot, bool isConstrained, bool isFixedHeight);
            [DllImport(module)]
            public static extern void Sixense_GetBaseConstrained(out bool c);

            [DllImport(module)]
            public static extern void Sixense_UpdateEyeTransform(PluginTypes.Vector pos, PluginTypes.Quat rot, bool isPositionTracked);
            [DllImport(module)]
            public static extern void Sixense_UpdateOriginTransform(PluginTypes.Vector pos, PluginTypes.Quat rot);

            [DllImport(module)]
            public static extern bool Sixense_SolveTracking();

            [DllImport(module)]
            public static extern void Sixense_GetEyeTransform(out PluginTypes.Vector pos, out PluginTypes.Quat rot);
            [DllImport(module)]
            public static extern void Sixense_GetTrackerTransform(Tracker track, out PluginTypes.Vector pos, out PluginTypes.Quat rot);
            [DllImport(module)]
            public static extern void Sixense_SetTrackerTransform(Tracker track, PluginTypes.Vector pos, PluginTypes.Quat rot);
            [DllImport(module)]
            public static extern void Sixense_GetTrackerData(Tracker track, out SixenseCore.PluginTypes.TrackedDeviceData data);

            [DllImport(module)]
            public static extern void Sixense_SetChainBlendWeight(JointChain chain, float weight);
            [DllImport(module)]
            public static extern void Sixense_GetChainBlendWeight(JointChain chain, out float weight);

            [DllImport(module)]
            public static extern void Sixense_HandleTeleport();

            [DllImport(module)]
            public static extern void Sixense_GetMovementDelta(out PluginTypes.Vector vec);
            #endregion

            #region Inverse Kinematics API
            [DllImport(module)]
            public static extern void Sixense_SetJointOffset(Tracker track, PluginTypes.Vector vec);
            [DllImport(module)]
            public static extern void Sixense_SetHandAccessoryBindOffset(Hand bind, Tracker track, PluginTypes.Vector vec, PluginTypes.Quat quat);

            [DllImport(module)]
            public static extern void Sixense_UpdateJointPositions(PluginTypes.Skeleton skel);
            [DllImport(module)]
            public static extern void Sixense_UpdateHingeAxis(JointChain chain, PluginTypes.Vector vec);

            [DllImport(module)]
            public static extern bool Sixense_SolveSkeleton();

            [DllImport(module)]
            public static extern void Sixense_GetJointPositions(out PluginTypes.Skeleton skel);
            [DllImport(module)]
            public static extern void Sixense_GetHingeAxis(JointChain chain, out PluginTypes.Vector vec);

            [DllImport(module)]
            public static extern void Sixense_GetTorsoRotation(out PluginTypes.Quat rot);
            #endregion
        }
        #endregion
    }
}