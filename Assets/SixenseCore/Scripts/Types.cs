//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseCore Unity Plugin
// Version 0.1
//

using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace SixenseCore
{
    #region Public Types
    public enum System
    { 
        UNKNOWN                              = 0,
        HYDRA                                = 1,
        STEM                                 = 2,
    }

    internal enum BaseStatus
    {
        NONE = 0,
        CONNECTED = (0x01 << 0),
    }
    
    /// <summary>
    /// Type of device
    /// </summary>
    public enum Hardware
    {
        UNKNOWN             = 0,

        // Razer Hydra
        HYDRA_CONTROLLER    = 1,

        // Sixense STEM System
        STEM_CONTROLLER     = 2,
        STEM_PACK           = 3,
    }

    /// <summary>
    /// Dock position tracker is bound to.
    /// </summary>
    public enum TrackerID
    {
        UNKNOWN             = 0,

        LEFT_CONTROLLER     = 1,
        RIGHT_CONTROLLER    = 2,
        LEFT_PACK           = 3,
        CENTER_PACK         = 4,
        RIGHT_PACK          = 5,

        // Additional devices that share the same dock as another active device
        LEFT_CONTROLLER_2   = 6,
        RIGHT_CONTROLLER_2  = 7,
        LEFT_PACK_2         = 8,
        CENTER_PACK_2       = 9,
        RIGHT_PACK_2        = 10,
    }

    /// <summary>
    /// Device status flags
    /// </summary>
    public enum Status
    { 
        ENABLED                                     = (0x01<<0),
        DOCKED                                      = (0x01<<1),
        HAS_EXTERNAL_POWER                          = (0x01<<2),
        BATTERY_CHARGING                            = (0x01<<3),
        BATTERY_LOW                                 = (0x01<<4),
        MODE_WIRED                                  = (0x01<<5),
        MODE_WIRELESS								= (0x01<<6),
        MODE_BLUETOOTH							    = (0x01<<7),
        MODE_WIFI									= (0x01<<8),
        FILTER_HEMI_TRACKING_ENABLED                = (0x01<<9),
        FILTER_MOVING_AVERAGE_ENABLED               = (0x01<<10),
        FILTER_IMU_DISTORTION_CORRECTION_ENABLED    = (0x01<<11),
    }
    
    /// <summary>
    /// Filter types
    /// </summary>
    public enum Filter
    {
        HEMI_TRACKING                             = 1,
        MOVING_AVERAGE                            = 2,
        IMU_DISTORTION_CORRECTION                 = 3,
    }

    /// <summary>
    /// Controller button mask.
    /// </summary>
    /// <remarks>
    /// The TRIGGER button is set when the Trigger value is greater than Tracker.TriggerButtonThreshold.
    /// </remarks>
    public enum Buttons
    {
        START       = (1<<0),
        
        PREV        = (1<<1),
        NEXT        = (1<<2),

        A           = (1<<3),
        B           = (1<<5),
        X           = (1<<6),
        Y           = (1<<4),
        
        BUMPER      = (1<<7),
        JOYSTICK    = (1<<8),

        TRIGGER     = (1<<9),
    }

    /// <summary>
    /// Parameters for the moving average filter
    /// 
    /// TODO: explain parameters
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct MovingAverageFilter
    {
        public float NearRange;
        public float NearValue;

        public float FarRange;
        public float FarValue;
    }
    #endregion

    #region Native Interface
    internal class PluginTypes
    {
        internal enum Result
        { 
            SUCCESS											= 0,
            FAILURE											= -1,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackedDeviceData
        {
            public byte sequence_number;
            public ushort tracker_id;
            public ushort base_index;
            public ushort tracked_device_index;
            public ulong packet_time;
            public byte dsp_gain;    
            public uint status;
            public byte battery_percent;
            public uint buttons;
            public float trigger;
            public float joystick_x;
            public float joystick_y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] pos;        
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] rot_quat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] tracker_pos;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] tracker_rot_quat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] imu_gravity;  
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public float[] rot_mat;  
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackedDeviceInfo
        {
            public byte runtime_version_major;    
            public byte runtime_version_minor;      
            public ushort hardware_revision;
            public ushort magnetic_frequency;
            public short tracked_device_index;       
            public byte device_type;
            public ushort serial_number;
            public byte mcu_firmware_version_major;
            public byte mcu_firmware_version_minor;
            public byte dsp_firmware_version_major;
            public byte dsp_firmware_version_minor;
            public byte board_hardware_gpio;
            public byte battery_adc_level;
            public float battery_voltage;
            public byte imu_calibration_status;
            public byte dsp_gain_mode;
            public byte active_matrix_set;   
        }
    }
    #endregion
}