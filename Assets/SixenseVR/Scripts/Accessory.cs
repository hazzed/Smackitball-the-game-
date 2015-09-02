//
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
    public class Accessory : MonoBehaviour
    {
        #region Public Members
        [Tooltip("ID of extra STEM pack to bind this accessory to.")]
        public Tracker m_pack = Tracker.EXTRA_1;

        [Tooltip("If true, bind this accessory to the offhand.")]
        public bool m_offhand = false;
        #endregion

        #region Prefab Transform Hooks
        public Transform m_sensor;
        #endregion

        #region Private Members
        static SixenseCore.Tracker ms_nullController = new SixenseCore.Tracker(null);

        SixenseCore.Tracker m_controller;
        #endregion

        #region Properties
        public SixenseCore.Tracker Input
        {
            set
            {
                m_controller = value;
            }
            get
            {
                if (m_controller != null)
                    return m_controller;

                return ms_nullController;
            }
        }

        public bool HasInput
        {
            get { return m_controller != null && m_controller.Enabled; }
        }
        #endregion
    }
}
