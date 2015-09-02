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
    [ExecuteInEditMode]
    public class FixedBase : MonoBehaviour
    {
        #region Public Members
        [Tooltip("Optional transform for a model or effect corresponding to the safe play area. Scale will be set to equal play area radius, origin set to play area center at floor height.")]
        public Transform m_safeFloor;
        #endregion

        #region Prefab Transform Hooks
        public Transform m_emitter;
        #endregion

        #region Private Members
        float m_radius = 1f;
        Vector3 m_center = new Vector3(0, 0, -1.2f);

        Vector3 m_startPos;
        Quaternion m_startRot;

        BodySolver m_solver = null;
        #endregion

        #region Script Events
        void Awake()
        {
            if (Application.isPlaying)
            {
                m_startPos = m_emitter.position;
                m_startRot = m_emitter.rotation;
            }
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                if (m_solver == null)
                {
                    var avs = GameObject.FindObjectsOfType<Avatar>();

                    foreach (var a in avs)
                    {
                        if (a.Solver.Index == 0)
                        {
                            m_solver = a.Solver;
                            a.m_fixedBase = this;
                            a.m_joystickLocomotion = false;
                        }
                    }
                }

                if (m_solver != null)
                    m_solver.ConstrainBaseTransform(m_startPos, m_startRot, true);

                m_radius = UserProfile.SafeAreaRadius;
                m_center = UserProfile.SafeAreaCenter;

                if (m_safeFloor != null)
                {
                    Vector3 pos = m_emitter.position + m_emitter.rotation * m_center;
                    m_safeFloor.position = new Vector3(pos.x, m_safeFloor.position.y, pos.z);
                    m_safeFloor.localScale = Vector3.one * m_radius;
                }
            }
        }
        #endregion
    }
}
