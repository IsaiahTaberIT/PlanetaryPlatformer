using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    private MovingPlatform platform;
    public override void OnInspectorGUI()
    {
        platform = (MovingPlatform)target;

        if (platform.SpinInEditor)
        {
            platform.TrySpinPlatform();

        }


        // Get a reference to the target script

        // This will automatically toggle open/closed without needing the boolean

        if (platform.Spin)
        {
            platform.DisplayFunctions = EditorGUILayout.Foldout(platform.DisplayFunctions, "Functions");

            if (platform.DisplayFunctions)
            {
                if (GUILayout.Button("Invert Spinning Direction"))
                {
                    platform.InvertSpinningDirection();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(platform);
                    serializedObject.Update();
                }
                GUILayout.Space(10);

                //    if (GUILayout.Button("Insert Segment"))
                //   {
                //      platform.InsertSegment();
                //    }
                // GUILayout.Space(10);
            }
        }
       


        DrawDefaultInspector();
 

    }
}
