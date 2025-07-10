using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR


//[CustomEditor(typeof(PlanetAlignmentScript))]
public class PlanetAlignmentScriptEditor : MonoBehaviour
{
    /*
    private Vector3 lastPosition;
    private void OnEnable()
    {
        SceneView.duringSceneGui += MyFunction;

    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= MyFunction;
    }

    private void MyFunction(SceneView sceneView)
    {
        PlanetAlignmentScript alignScript = (PlanetAlignmentScript)target;

        if (alignScript == null) return;

        if (alignScript.transform.position != lastPosition)
        {
            lastPosition = alignScript.transform.position;
            alignScript.Align();
        }
    }
        */

}
#endif


