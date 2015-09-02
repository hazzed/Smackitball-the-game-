//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseCore Unity Plugin
// Version 0.1
//

using UnityEngine;
using System.Collections.Generic;

namespace SixenseCore
{
    [SelectionBase]
    public class BaseVisual : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("Type of system for this prefab.")]
        public SixenseCore.System m_systemType;

        [Tooltip("Set of prefabs for each tracking device type.")]
        public TrackerVisual[] m_trackerPrefabs;

        [Tooltip("3D Model for this system base.")]
        public Transform m_model;

        [Tooltip("Transform defining the position and orientation of the sensor hardware within the base station.")]
        public Transform m_emitter;
        #endregion

        #region Private Variables
        TrackerVisual[] m_trackers = null;
        #endregion

        #region 3D Visualization
        void Start()
        {
            if(!SixenseCore.Device.Initialized)
            {
                Debug.LogWarning("SixenseCore not initialized.");
                enabled = false;
                return;
            }

            m_trackers = new TrackerVisual[SixenseCore.Device.MaxNumberTrackers];
        }

        void UpdateVisual()
        {
            if(SixenseCore.Device.BaseConnected != m_model.gameObject.activeSelf)
            {
                m_model.gameObject.SetActive(SixenseCore.Device.BaseConnected && SixenseCore.Device.SystemType == m_systemType);
            }

            for (int i = 0; i < m_trackers.Length; i++)
            {
                var c = Device.GetTrackerByIndex(i);
                if (!c.Enabled)
                {
                    if (m_trackers[i] != null)
                    {
                        Destroy(m_trackers[i].gameObject);
                    }
                    continue;
                }

                if (m_trackers[i] == null)
                {
                    int index = -1;
                    for(int j=0; j<m_trackerPrefabs.Length;j++)
                    {
                        if (m_trackerPrefabs[j] == null)
                            continue;

                        if(m_trackerPrefabs[j].m_type == c.HardwareType)
                        {
                            index = j;
                            break;
                        }
                    }
                    if (index == -1)
                        continue;

                    var go = GameObject.Instantiate(m_trackerPrefabs[index].gameObject) as GameObject;
                    var con = go.GetComponent<TrackerVisual>();
                    
                    go.transform.parent = transform;
                    m_trackers[i] = con;
                }
                else if (m_trackers[i].m_type != c.HardwareType && c.HardwareType != Hardware.UNKNOWN)
                {
                    Debug.LogWarning("Device type mismatch: " + i + " Expected " + m_trackers[i].m_type + " Got " + c.HardwareType);
                    Destroy(m_trackers[i].gameObject);
                    continue;
                }

                {
                    var sensor = m_trackers[i].m_sensor;
                    var rot = Quaternion.Inverse(sensor.rotation) * (m_trackers[i].transform.rotation);

                    m_trackers[i].transform.rotation = m_emitter.rotation * c.Rotation * rot;

                    var pos = sensor.position - m_trackers[i].transform.position;

                    m_trackers[i].transform.position = m_emitter.TransformPoint(c.Position) - pos;
                }
            }
        }

        void FixedUpdate()
        {
            if (!SixenseCore.Device.Initialized)
            {
                return;
            }

            UpdateVisual();
        }
        void Update()
        {
            if (!SixenseCore.Device.Initialized)
            {
                return;
            }

            for (int i = 0; i < m_trackers.Length; i++)
            {
                if (m_trackers[i] == null)
                    continue;

                var c = Device.GetTrackerByIndex(i);
                m_trackers[i].gameObject.SetActive(c.Enabled);

                if (!c.Enabled)
                    continue;
            }

            UpdateVisual();
        }
        #endregion
    }
}
