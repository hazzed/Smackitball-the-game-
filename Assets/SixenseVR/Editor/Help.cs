//
// Copyright (C) 2014 Sixense Entertainment Inc.
// All Rights Reserved
//
// SixenseVR Unity Plugin
// Version 0.2
//

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SixenseVR
{
    public class Help : EditorWindow
    {
        static bool ms_hasText;
        static string ms_text = "";

        static bool ms_hasVersion;
        static string version = "";

        Vector2 scroll;

        [MenuItem("Help/About SixenseVR...")]
        public static Help Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (Help)EditorWindow.GetWindow(typeof(Help));
            window.title = "About";

            return window;
        }

        void OnGUI()
        {
            if(!ms_hasText)
            {
                ms_text = File.ReadAllText("Assets/SixenseVR/Readme.txt");

                ms_hasText = true;
            }
            if (!ms_hasVersion)
            {
                version = "SixenseVR API " + SixenseVR.Device.APIVersion;

                ms_hasVersion = true;
            }

            UpdateGUI();

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(version);
        }

        void UpdateGUI()
        {
            GUILayout.Label("SixenseVR SDK Copyright 2014 Sixense Entertainment Inc.");
            scroll = GUILayout.BeginScrollView(scroll);
            {
                GUILayout.TextArea(ms_text);
            }
            GUILayout.EndScrollView();

            if(GUILayout.Button("Support Forum"))
            {
                System.Diagnostics.Process.Start("http://sixense.com/forum/vbulletin/");
            }
        }
    }
}