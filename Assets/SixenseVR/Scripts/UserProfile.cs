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
    public class UserProfile
    {
        #region Properties
        /// <summary>
        /// Vector offset of the STEM Pack from the center eye point on the HMD
        /// </summary>
        public static Vector3 HeadOffset
        {
            get
            {
                PluginTypes.Vector v;
                Plugin.Sixense_GetHeadOffset(out v);
                return PluginTypes.ToUnityVector(v);
            }
            set
            {
                Plugin.Sixense_SetHeadOffset(PluginTypes.ToSixenseVector(value));
            }
        }

        /// <summary>
        /// Does the user wish to have a male or female avatar?
        /// </summary>
        public static Gender GenderPreference
        {
            get
            {
                Gender g;
                Plugin.Sixense_GetGenderPreference(out g);
                return g;
            }
            set
            {
                Plugin.Sixense_SetGenderPreference(value);
            }
        }

        /// <summary>
        /// Does the user have a dominant hand?
        /// </summary>
        public static Hand Handedness
        {
            get
            {
                Hand h;
                Plugin.Sixense_GetHandedness(out h);
                return h;
            }
            set
            {
                Plugin.Sixense_SetHandedness(value);
            }
        }

        /// <summary>
        /// The user's standing height
        /// </summary>
        public static float Height
        {
            get
            {
                float h;
                Plugin.Sixense_GetHeight(out h);
                return h;
            }
            set
            {
                Plugin.Sixense_SetHeight(value);
            }
        }

        /// <summary>
        /// The user's full armspan
        /// </summary>
        public static float ArmSpan
        {
            get
            {
                float s;
                Plugin.Sixense_GetArmSpan(out s);
                return s;
            }
            set
            {
                Plugin.Sixense_SetArmSpan(value);
            }
        }

        /// <summary>
        /// Distance from the bottom of the STEM Base to the floor
        /// </summary>
        public static float FloorDistance
        {
            get
            {
                float d;
                Plugin.Sixense_GetArmSpan(out d);
                return d;
            }
            set
            {
                Plugin.Sixense_SetArmSpan(value);
            }
        }

        /// <summary>
        /// Approximate difference between user's sitting and standing height, determines camera height while sitting
        /// </summary>
        public static float ChairOffset
        {
            get
            {
                float o;
                Plugin.Sixense_GetArmSpan(out o);
                return o;
            }
            set
            {
                Plugin.Sixense_SetArmSpan(value);
            }
        }

        /// <summary>
        /// Vector offset from STEM Base indicating the center of the play area
        /// </summary>
        public static Vector3 SafeAreaCenter
        {
            get
            {
                PluginTypes.Vector v;
                float r;
                Plugin.Sixense_GetSafeArea(out v, out r);
                return PluginTypes.ToUnityVector(v);
            }
            set
            {
                PluginTypes.Vector v;
                float r;
                Plugin.Sixense_GetSafeArea(out v, out r);
                Plugin.Sixense_SetSafeArea(PluginTypes.ToSixenseVector(value), r);
            }
        }

        /// <summary>
        /// Radius of allowed play area around the center point, should be clear of any obstacles
        /// </summary>
        public static float SafeAreaRadius
        {
            get
            {
                PluginTypes.Vector v;
                float r;
                Plugin.Sixense_GetSafeArea(out v, out r);
                return r;
            }
            set
            {
                PluginTypes.Vector v;
                float r;
                Plugin.Sixense_GetSafeArea(out v, out r);
                Plugin.Sixense_SetSafeArea(v, value);
            }
        }

        /// <summary>
        /// Minimum distance from the base at which the controllers will be visible inside VR
        /// </summary>
        public static float VisibleBaseDistance
        {
            get
            {
                float d;
                Plugin.Sixense_GetVisibleBaseDistance(out d);
                return d;
            }
            set
            {
                Plugin.Sixense_SetVisibleBaseDistance(value);
            }
        }
        #endregion

        #region Saved Data
        public static void Load()
        {
            if (!Plugin.Sixense_Load())
                Debug.LogError("SixenseVR Load Calibration Failed");
        }

        public static void Save()
        {
            if (!Plugin.Sixense_Save())
                Debug.LogError("SixenseVR Save Calibration Failed");
        }

        public static void Reset()
        {
            if (Plugin.Sixense_Reset())
                Debug.LogError("SixenseVR Reset Calibration Failed");
        }
        #endregion

        #region Native Interface
        private partial class Plugin
        {
            const string module = "SixenseVR";

            [DllImport(module)]
            public static extern bool Sixense_Load();
            [DllImport(module)]
            public static extern bool Sixense_Save();
            [DllImport(module)]
            public static extern bool Sixense_Reset();

            [DllImport(module)]
            public static extern void Sixense_SetHeadOffset(PluginTypes.Vector vec);
            [DllImport(module)]
            public static extern void Sixense_SetGenderPreference(Gender g);
            [DllImport(module)]
            public static extern void Sixense_SetHandedness(Hand h);
            [DllImport(module)]
            public static extern void Sixense_SetHeight(float h);
            [DllImport(module)]
            public static extern void Sixense_SetArmSpan(float s);
            [DllImport(module)]
            public static extern void Sixense_SetFloorDistance(float h);
            [DllImport(module)]
            public static extern void Sixense_SetChairOffset(float h);
            [DllImport(module)]
            public static extern void Sixense_SetSafeArea(PluginTypes.Vector center, float radius);
            [DllImport(module)]
            public static extern void Sixense_SetVisibleBaseDistance(float d);

            [DllImport(module)]
            public static extern void Sixense_GetHeadOffset(out PluginTypes.Vector vec);
            [DllImport(module)]
            public static extern void Sixense_GetGenderPreference(out Gender g);
            [DllImport(module)]
            public static extern void Sixense_GetHandedness(out Hand h);
            [DllImport(module)]
            public static extern void Sixense_GetArmSpan(out float s);
            [DllImport(module)]
            public static extern void Sixense_GetHeight(out float h);
            [DllImport(module)]
            public static extern void Sixense_GetFloorDistance(out float d);
            [DllImport(module)]
            public static extern void Sixense_GetChairOffset(out float o);
            [DllImport(module)]
            public static extern void Sixense_GetSafeArea(out PluginTypes.Vector center, out float radius);
            [DllImport(module)]
            public static extern void Sixense_GetVisibleBaseDistance(out float d);
        }
        #endregion
    }
}
