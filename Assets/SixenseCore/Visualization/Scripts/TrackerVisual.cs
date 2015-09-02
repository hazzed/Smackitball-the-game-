//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseCore Unity Plugin
// Version 0.1
//

using UnityEngine;
using System.Collections;

namespace SixenseCore
{
    [SelectionBase]
    public class TrackerVisual : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("Specifies the hardware identifying type for this tracking device.")]
        public SixenseCore.Hardware m_type;

        [Tooltip("Transform that defines the position and orientation of the sensor hardware within the tracked device.")]
        public Transform m_sensor;
        #endregion

        #region Private Variables
        static Tracker ms_nullController = new Tracker(null);

        Tracker m_controller = null;
        #endregion

        #region Properties
        
        public Tracker Input
        {
            get
            {
                if (m_controller != null)
                    return m_controller;

                return ms_nullController;
            }
            set
            {
                m_controller = value;
            }
        }

        public bool HasInput
        {
            get { return m_controller != null && m_controller.Connected; }
        }
        #endregion
    }
}
