
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(UltimateCurveGenerationScript))]
public class UltimateCurveGenerationScriptEditor : Editor
{

    private UltimateCurveGenerationScript myComponent;
 

    public override void OnInspectorGUI()
    {
        int counter = 0;
        int SliderIndex = 0;
        bool displayedSlider = false;

        EditorGUI.BeginChangeCheck();


        serializedObject.Update();
        // Get a reference to the target script
        myComponent = (UltimateCurveGenerationScript)target;

        // This will automatically toggle open/closed without needing the boolean

        myComponent.showGizmoSettings = EditorGUILayout.Foldout(myComponent.showGizmoSettings, "Gizmo Settings");

        if (myComponent.showGizmoSettings)
        {
            myComponent.SelectedColor = EditorGUILayout.ColorField("SelectedColor", myComponent.SelectedColor);
            myComponent.UnSelectedColor = EditorGUILayout.ColorField("UnSelectedColor", myComponent.UnSelectedColor);
            myComponent.GizmoSize = EditorGUILayout.FloatField("Gizmo Size", myComponent.GizmoSize);
            myComponent.DrawAny = EditorGUILayout.Toggle("DrawAny", myComponent.DrawAny);
            myComponent.DrawAll = EditorGUILayout.Toggle("DrawAll", myComponent.DrawAll);
        }

        myComponent.showButtons = EditorGUILayout.Foldout(myComponent.showButtons, "Segment Controls");

        if (myComponent.showButtons)
        {
            if (GUILayout.Button("Delete Current Segment"))
            {
                myComponent.DeleteSegment();
            }
            GUILayout.Space(5);

            if (GUILayout.Button("Insert Segment"))
            {
                myComponent.InsertSegment();
            }
            GUILayout.Space(10);
        }
       


        // The "true" here makes it always expanded, just toggle this.
        // If you want to show the settings when folded out




        // Cache all visible properties
        List<SerializedProperty> properties = new List<SerializedProperty>();
        SerializedProperty property = serializedObject.GetIterator();


        property.NextVisible(true); // Skip m_Script


        while (property.NextVisible(false))
        {
            // Exclude CurrentSegment (we'll draw it manually with a slider)
            if (property.name != "CurrentSegment")
            {
                properties.Add(property.Copy());
            }
            counter++;
          //  Debug.Log(counter);
        }


      //  Debug.Log(properties.Count);

        for (int i = 0; i < properties.Count; i++)
        {
            if ((i == SliderIndex || properties.Count <= SliderIndex) && !displayedSlider) 
            {
                // Draw the slider manually
                displayedSlider = true;
                myComponent.CurrentSegment = (int)EditorGUILayout.Slider("Current Segment", myComponent.CurrentSegment, 0, myComponent.Segments.Count - 1);
                
            }

            EditorGUILayout.PropertyField(properties[i], true);
        //    Debug.Log(properties[i].name);

        }

        serializedObject.ApplyModifiedProperties();




    



        EditorGUI.EndChangeCheck();
        {
            UnityEditor.SceneView.RepaintAll();
            if (myComponent.CurrentSegment != myComponent.LastSegment)
            {
                myComponent.GenerateSpriteShape();
            }

        }











    }

    void OnSceneGUI()
    {



        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (InternalEditorUtility.GetIsInspectorExpanded(myComponent))
            {
              //  InternalEditorUtility.SetIsInspectorExpanded(target, true);
                myComponent.Clicked(Event.current.mousePosition);
                e.Use();
            }
        }
    }

}
#endif
