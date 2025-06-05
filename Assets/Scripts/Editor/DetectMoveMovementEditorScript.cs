
using UnityEditor;
using UnityEngine;

public abstract class DetectMoveMovementEditorScript<T> : Editor where T : MonoBehaviour
{
    private Vector3 lastPosition;

    protected virtual void OnEnable()
    {
        lastPosition = ((T)target).transform.position;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        T script = (T)target;
        Transform transform = script.transform;

        if (transform.position != lastPosition)
        {
            OnTransformMoved(script);
            lastPosition = transform.position;
        }
    }
    protected abstract void OnTransformMoved(T script);
}
