using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Path))]
public class PathsEditor : Editor
{
    private void OnSceneGUI()
    {
        Path TargetPath = (Path)target;
        for (int i = 0; i < TargetPath.PathLocal.Count; i++)
        {
            // Transform local point to world space
            Vector3 worldPos = TargetPath.PathLocal[i] + TargetPath.transform.position;

            // Draw and update handle

            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            Undo.RecordObject(TargetPath, "Move Point");
            TargetPath.Updated = true;
            TargetPath.PathLocal[i] = newWorldPos - TargetPath.transform.position;
            TargetPath.PathWorld[i] = newWorldPos;

        }
    }
}

