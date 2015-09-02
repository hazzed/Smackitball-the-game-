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
    #region Public Types
    public enum Joint
    {
        ROOT = 0,
        SPINE,
        NECK,

        SHOULDER_LEFT,
        ELBOW_LEFT,
        WRIST_LEFT,

        SHOULDER_RIGHT,
        ELBOW_RIGHT,
        WRIST_RIGHT,

        HIP_LEFT,
        KNEE_LEFT,
        ANKLE_LEFT,

        HIP_RIGHT,
        KNEE_RIGHT,
        ANKLE_RIGHT,

        COUNT
    };

    public enum JointChain
    {
        ARM_LEFT = 0,
        ARM_RIGHT,

        LEG_LEFT,
        LEG_RIGHT,

        COUNT
    };

    public enum Tracker
    {
        BASE = 0,

        LEFT,
        RIGHT,

        HEAD,

        EXTRA_1,
        EXTRA_2,
        EXTRA_3,
        EXTRA_4,

        BODY_1,
        BODY_2,
        BODY_3,
        BODY_4,

        COUNT
    };

    public enum Hand
    {
        UNKNOWN = 0,

        LEFT,
        RIGHT
    };

    public enum Gender
    {
        UNKNOWN = 0,

        MALE,
        FEMALE
    };
    #endregion

    #region Native Interface
    internal class PluginTypes
    {
        const string module = "SixenseVR";

        // Vector space: meters, left handed, Y up

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vector
        {
            public float x, y, z;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Quat
        {
            public float x, y, z, w;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Skeleton
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Joint.COUNT)]
            public Vector[] joints;
        };

        public static Vector3 ToUnityVector(Vector v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }
        public static Quaternion ToUnityQuat(Quat q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }
        public static Vector ToSixenseVector(Vector3 v)
        {
            Vector sv;
            sv.x = v.x;
            sv.y = v.y;
            sv.z = -v.z;
            return sv;
        }
        public static Quat ToSixenseQuat(Quaternion q)
        {
            Quat sq;
            sq.x = q.x;
            sq.y = q.y;
            sq.z = -q.z;
            sq.w = -q.w;
            return sq;
        }
    }
    #endregion
}
