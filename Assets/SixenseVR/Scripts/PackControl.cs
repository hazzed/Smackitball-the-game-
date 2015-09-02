//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEngine;
using System.Collections.Generic;

namespace SixenseVR
{
    [SelectionBase]
    public class PackControl : MonoBehaviour
    {
        #region Public Members
        public Tracker m_tracker;

        public Transform m_sensor;

        public SixenseCore.Tracker m_controller = null;
        #endregion
    }
}
