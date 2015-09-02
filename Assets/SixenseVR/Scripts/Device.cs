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
    public class Device
    {
        #region Properties
        /// <summary>
        /// Returns a version string for the VR API
        /// </summary>
        public static string APIVersion
        {
            get 
            {
                CorePlugin.sxCoreGetAPIVersion(); // make sure the Core DLL is loaded first
                return Marshal.PtrToStringAnsi(Plugin.SixenseVR_API_VersionString());
            }
        }

        /// <summary>
        /// Returns true if the device has been initialized and is ready to use
        /// </summary>
        public static bool Initialized
        {
            get { return ms_initialized; }
        }
        #endregion

        #region Entry Point
        /// <summary>
        /// Initializes both the Sixense driver and SixenseVR runtime
        /// </summary>
        public static bool Init()
        {
            if (!ms_initialized)
            {
                Debug.Log("SixenseVR Init");
                
                if (CorePlugin.sxCoreInit() != SixenseCore.PluginTypes.Result.SUCCESS)
                {
                    Debug.LogError("Sixense Driver Init Failed");
                    return false;
                }
                if (!Plugin.SixenseVR_Init())
                {
                    Debug.LogError("SixenseVR Init Failed");
                    return false;
                }
            }

            ms_initialized = true;
            return true;

        }

        /// <summary>
        /// Shuts down both the Sixense driver and SixenseVR runtime 
        /// </summary>
        public static bool Shutdown()
        {
            if (ms_initialized)
            {
                if (!Plugin.SixenseVR_Shutdown())
                {
                    Debug.LogError("SixenseVR Shutdown Failed");
                    return false;
                }
                if (CorePlugin.sxCoreExit() != SixenseCore.PluginTypes.Result.SUCCESS)
                {
                    Debug.LogError("Sixense Driver Shutdown Failed");
                    return false;
                }

                Debug.Log("SixenseVR Shutdown");
            }

            ms_initialized = false;
            return true;
        }
        #endregion

        #region Private Members
        private static bool ms_initialized = false;
        #endregion

        #region Native Interface
        private partial class CorePlugin
        {
            const string module = "sxCore";

            [DllImport(module)]
            public static extern IntPtr sxCoreGetAPIVersion();

            [DllImport(module)]
            public static extern SixenseCore.PluginTypes.Result sxCoreInit();
            [DllImport(module)]
            public static extern SixenseCore.PluginTypes.Result sxCoreExit();
        }

        private partial class Plugin
        {
            const string module = "SixenseVR";

            [DllImport(module)]
            public static extern IntPtr SixenseVR_API_VersionString();

            [DllImport(module)]
            public static extern bool SixenseVR_Init();
            [DllImport(module)]
            public static extern bool SixenseVR_Shutdown();
        }
        #endregion
    }
}
