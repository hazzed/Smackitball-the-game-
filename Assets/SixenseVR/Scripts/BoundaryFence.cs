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
    public class BoundaryFence : MonoBehaviour
    {
        #region Public Members
        public Avatar m_avatar;
        public BodySolver m_solver;
        public float m_playRadius = 1.5f;
        public Vector3 m_playCenter = new Vector3(0, 0, -1.5f);
        public float m_warnDistance = 0.75f;

        public GameObject m_warningPrefab;
        public Transform m_safeFloor;
        #endregion

        #region Private Members
        Renderer[] m_warning;
        #endregion

        #region Script Events
        void Start()
        {
            m_warning = new Renderer[3];

            for (int i = 0; i < m_warning.Length; i++)
            {
                var go = GameObject.Instantiate(m_warningPrefab) as GameObject;

                go.transform.parent = transform;

                m_warning[i] = go.GetComponent<Renderer>();
                m_warning[i].enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            m_playCenter = UserProfile.SafeAreaCenter;
            m_playRadius = UserProfile.SafeAreaRadius;

            for (int i = 0; i < m_warning.Length; i++)
            {
                var w = m_warning[i];
                float wdist = m_warnDistance;

                Vector3 center = transform.position + transform.rotation * m_playCenter;

                Vector3 track;
                Quaternion r;

                if (i == 0)
                {
                    m_solver.GetTrackerTransform(Tracker.LEFT, out track, out r);
                    var con = m_avatar.GetInput(Tracker.LEFT);
                    if (con == null)
                    {
                        w.enabled = false;
                        continue;
                    }
                    w.transform.localScale = Vector3.one * 0.5f;
                    wdist *= 0.5f;
                }
                else if (i == 1)
                {
                    m_solver.GetTrackerTransform(Tracker.RIGHT, out track, out r);
                    var con = m_avatar.GetInput(Tracker.RIGHT);
                    if (con == null)
                    {
                        w.enabled = false;
                        continue;
                    }
                    w.transform.localScale = Vector3.one * 0.5f;
                    wdist *= 0.5f;
                }
                else
                {
                    var con = m_avatar.GetInput(Tracker.HEAD);
                    if (con == null)
                    {
                        w.enabled = false;
                        continue;
                    }
                    m_solver.GetEyeTransform(out track, out r);
                    w.transform.localScale = Vector3.one * 1f;
                }

                Vector3 delta = track - center;

                delta.y = 0;

                float dist = m_playRadius * m_playRadius - delta.sqrMagnitude;
                float t = 1f - dist / (m_playRadius * m_playRadius);

                if (t > 0.1f && m_playRadius != 0)
                {
                    w.enabled = true;

                    delta.Normalize();

                    Vector3 pos = delta * m_playRadius + center;
                    pos.y = track.y;

                    w.transform.position = pos;
                    w.transform.LookAt(track);

                    float heading = Mathf.Atan(delta.z / delta.x);
                    if (delta.x < 0)
                        heading += Mathf.PI;

                    float arc = heading * m_playRadius;
                    float height = pos.y;

                    if (t > 1)
                    {
                        arc *= -1;
                    }

                    float bright = wdist - dist;
                    MaterialPropertyBlock block = new MaterialPropertyBlock();

                    if (t > 1 && i == 2)
                        block.AddColor("_TintColor", new Color(0, 1, 0, 0.1f));
                    else
                        block.AddColor("_TintColor", new Color(1, 0, 0, Mathf.Clamp01(bright) * 0.75f));

                    block.AddVector("_GridOffset", new Vector4(arc, height, w.transform.lossyScale.x, w.transform.lossyScale.y));

                    w.SetPropertyBlock(block);
                }
                else
                    w.enabled = false;
            }
        }
        #endregion
    }
}
