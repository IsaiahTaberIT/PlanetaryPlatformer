using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(AutoProfileScript))]


public class CustomAutoProfileEditorScript : Editor
{

    private AutoProfileScript myComponent;
    private object Target;



    public int Max = 10;
    public override void OnInspectorGUI()
    {


        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        myComponent = (AutoProfileScript)target;
        Target = target;


        // Display a slider for the "MyValue" variable



        if (EditorGUI.EndChangeCheck())
        {
            myComponent.Repair();
        }


    }

}
#endif


