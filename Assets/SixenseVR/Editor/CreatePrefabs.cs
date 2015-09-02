using UnityEditor;
using UnityEngine;

namespace SixenseVR
{
    public class CreatePrefabs
    {
        // Add menu item
        [MenuItem("GameObject/Create Other/SixenseVR Grab Point Child")]

        static void Grab()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject)
            {
                Object obj = AssetDatabase.LoadAssetAtPath("Assets/SixenseVR/Prefabs/SixenseGrabPoint.prefab", typeof(UnityEngine.Object));
                GameObject grab = PrefabUtility.InstantiatePrefab(obj) as GameObject;

                if (grab != null)
                {
                    grab.transform.parent = selectedObject.transform;
                    grab.transform.position = selectedObject.transform.position;
                    grab.transform.rotation = selectedObject.transform.rotation;

                    Selection.activeObject = grab;
                    SceneView.lastActiveSceneView.FrameSelected();
                    SceneView.RepaintAll();
                }
            }
        }

        // Add menu item
        [MenuItem("GameObject/Create Other/SixenseVR Accessory Child")]

        static void Accessory()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject)
            {
                Object obj = AssetDatabase.LoadAssetAtPath("Assets/SixenseVR/Prefabs/SixenseAccessory.prefab", typeof(UnityEngine.Object));
                GameObject grab = PrefabUtility.InstantiatePrefab(obj) as GameObject;

                if (grab != null)
                {
                    grab.transform.parent = selectedObject.transform;
                    grab.transform.position = selectedObject.transform.position;
                    grab.transform.rotation = selectedObject.transform.rotation;

                    Selection.activeObject = grab;
                    SceneView.lastActiveSceneView.FrameSelected();
                    SceneView.RepaintAll();
                }
            }
        }

        // Add menu item
        [MenuItem("GameObject/Create Other/SixenseVR Fixed Base Station")]

        static void Base()
        {
            Object obj = AssetDatabase.LoadAssetAtPath("Assets/SixenseVR/Prefabs/SixenseFixedBase.prefab", typeof(UnityEngine.Object));
            GameObject grab = PrefabUtility.InstantiatePrefab(obj) as GameObject;

            if (grab != null)
            {
                Selection.activeObject = grab;
                SceneView.lastActiveSceneView.FrameSelected();
                SceneView.RepaintAll();
            }
        }
    }
}
