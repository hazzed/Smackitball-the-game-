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
    public class GrabPoint : MonoBehaviour
    {
        #region Public Members
        [Tooltip("If true, the trigger must remain pressed to hold on to this object.")]
        public bool m_dropOnRelease = false;

        [Tooltip("If set, this grab point can only be grabbed by the specified hand.")]
        public Hand m_handedness = Hand.UNKNOWN;

        [Tooltip("Maximum distance at which this point can be grabbed.  The hand must also be in contact with the object collider.")]
        public float m_grabRange = 0.2f;

        [Tooltip("Maximum angle difference between hand and grab controller.")]
        [Range(10, 180)]
        public float m_grabMaxAngle = 45;

        [Tooltip("If true, the grabbed object is treated as a cylinder, and its rotation along the Z axis of the GripCenter transform is not considered for the maximum angle.")]
        public bool m_radialSymmetry = false;

        [Tooltip("Animator variable to set when this object is held.")]
        public string m_animationTransition = "Grab";
        #endregion

        #region Prefab Transform Hooks
        public Transform m_grabAnchor;
        public Transform m_sensor;
        #endregion

        #region Private Members
        static SixenseCore.Tracker ms_nullController = new SixenseCore.Tracker(null);

        SixenseCore.Tracker m_controller;
        HandControl m_hand;

        bool m_dropButton;
        float m_dropButtonTime;
        #endregion

        #region Properties
        public SixenseCore.Tracker Input
        {
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

        public HandControl HandControl
        {
            get { return m_hand; }
            set
            {
                m_hand = value;
                m_controller = (value != null ? value.m_controller : null);
            }
        }

        public Transform Anchor
        {
            get
            {
                if (m_grabAnchor != null)
                    return m_grabAnchor;

                return transform;
            }
        }
        #endregion

        #region Public Methods
        public void Drop()
        {
            if (m_hand != null)
            {
                m_hand.Drop();
            }
        }

        public Vector3 GrabPosition(Hand hand)
        {
            if (m_handedness == Hand.UNKNOWN || m_handedness == hand)
            {
                return m_sensor.position;
            }

            return Vector3.zero;
        }

        public Quaternion GrabRotation(Hand hand)
        {
            if (m_handedness == Hand.UNKNOWN || m_handedness == hand)
            {
                return m_sensor.rotation;
            }

            return Quaternion.identity;
        }
        #endregion

        #region Editor Visualization
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 1, 0.3f);

            Gizmos.matrix = Matrix4x4.TRS(Anchor.position, Anchor.rotation, m_grabRange * Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, 1);

            if (m_grabMaxAngle < 180)
            {
                Gizmos.DrawFrustum(Vector3.zero, m_grabMaxAngle * 2, -Mathf.Cos(Mathf.Deg2Rad * m_grabMaxAngle), 0, 1);
            }
        }
        #endregion
    }
}
