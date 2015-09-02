//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEngine;
using System.Collections;
using System.Reflection;

namespace SixenseVR
{
    [ExecuteInEditMode]
    public class VRCameraSetup : MonoBehaviour
    {
        #region Configuration
        const int c_hiddenLayer = 31;
        const float c_nearPlane = 0.02f;
        #endregion

        #region Public Methods
        public static GameObject CreateCamera(Transform follow)
        {
#if UNITY_EDITOR
            var prefab = FindCameraPrefab();

            if (prefab == null)
            {
                Debug.LogWarning("Please install the Oculus SDK");

                return null;
            }

            var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            go.transform.parent = follow;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            bool found = false;

            var components = go.GetComponentsInChildren<MonoBehaviour>();

            foreach (var m in components)
            {
                if (m == null)
                    continue;

                var type = m.GetType();

                /************* OCULUS RIFT *******************/
                if (type.Name == "OVRCameraRig")
                {
                    m.transform.localPosition = new Vector3(0, 1.6f, 0);

                    foreach (var c in go.GetComponentsInChildren<Camera>())
                    {
                        c.nearClipPlane = c_nearPlane;
                        c.cullingMask = ~(1 << c_hiddenLayer);
                    }

                    found = true;
                }
                else if (type.Name == "OVRCameraController")
                {
                    FieldInfo[] fields = type.GetFields();
                    foreach (var f in fields)
                    {
                        if (f.Name.ToString() == "TrackerRotatesY")
                        {
                            f.SetValue(m, true);
                        }
                        if (f.Name.ToString() == "FollowOrientation")
                        {
                            f.SetValue(m, follow);
                        }
                    }

                    PropertyInfo[] props = type.GetProperties();
                    foreach (var p in props)
                    {
                        if (p.Name.ToString() == "NearClipPlane")
                        {
                            p.SetValue(m, c_nearPlane, null);
                        }
                    }

                    foreach (var c in go.GetComponentsInChildren<Camera>())
                    {
                        c.transform.localPosition = new Vector3(0, 1.6f, 0);
                        c.nearClipPlane = c_nearPlane;
                        c.cullingMask = ~(1 << c_hiddenLayer);
                    }

                    found = true;
                }
                /*********************************************/
            }

            if (!found)
            {
                Debug.LogWarning("Prefab " + prefab.name + " does not contain the expected properties and may function incorrectly, please update your SixenseVR SDK to ensure compatibility");
            }

            return go;
#else
        return null;
#endif
        }

        public static GameObject FindCameraPrefab()
        {
#if UNITY_EDITOR
            var assets = UnityEditor.AssetDatabase.FindAssets("OVRCameraController");

            GameObject prefab = null;

            if (assets.Length > 0)
            {
                foreach (var a in assets)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(a);

                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

                    if (prefab != null)
                    {
                        return prefab;
                    }
                }
            }

            assets = UnityEditor.AssetDatabase.FindAssets("OVRCameraRig");

            if (assets.Length > 0)
            {
                foreach (var a in assets)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(a);

                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

                    if (prefab != null)
                    {
                        return prefab;
                    }
                }
            }
#endif

            return null;
        }
        #endregion

        #region Private Methods
        bool FindCamera()
        {
            bool found = false;

            var components = gameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (var c in components)
            {
                if (c == null)
                    continue;

                var type = c.GetType();

                if (type.Name == "OVRCameraController")
                {
                    found = true;
                }
                if (type.Name == "OVRCameraRig")
                {
                    found = true;
                }
            }

            return found;
        }
        #endregion

        #region Monobehavior Based Setup
        void OnEnable()
        {
#if UNITY_EDITOR
            if (FindCamera())
            {
                return;
            }
            else
            {
#if !NO_OCULUS
                GameObject go = CreateCamera(transform);
#endif
            }
#endif
        }
        #endregion
    }
}
