
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
[CustomEditor(typeof(AdvancedCurveGenerationScript))]

public class CustomSliderEditor : Editor
{
    
    private AdvancedCurveGenerationScript myComponent;
    private object Target;

    public int Max = 10;
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();
        myComponent = (AdvancedCurveGenerationScript)target;
        Target = target;
        myComponent._CurrentSegment = (int)EditorGUILayout.Slider("Current Segment", myComponent._CurrentSegment, 0, myComponent.SegmentCount - 1);

        if (myComponent._CurrentSegment != myComponent._LastSegment)
        myComponent.CallValidatePlease();

        if (EditorGUI.EndChangeCheck())
        {
            myComponent.Repair();
        }
    }

    void OnSceneGUI()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (InternalEditorUtility.GetIsInspectorExpanded(myComponent))
            {
                myComponent.Clicked(Event.current.mousePosition);
            }
        }
    }

}
#endif


