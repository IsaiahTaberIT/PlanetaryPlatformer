using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class MovementWaterEditor : Editor
{
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Vector3 LastRotation;
    private void OnEnable()
    {
        if (target is MonoBehaviour)
        {
            lastPosition = ((MonoBehaviour)target).transform.position;
            lastScale = ((MonoBehaviour)target).transform.localScale;
            LastRotation = ((MonoBehaviour)target).transform.localEulerAngles;

        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target is MonoBehaviour)
        {
            var mono = (MonoBehaviour)target;
            var currentPosition = mono.transform.position;
            var currentScale = mono.transform.localScale;
            var currentRotation = mono.transform.localEulerAngles;

            if (currentScale != lastScale)
            {
                lastScale = currentScale;
           
                InvokeOnEditorTransformModifiedMethods(mono, typeof(OnEditorScaledAttribute));
            }

            if (currentPosition != lastPosition)
            {
                LastRotation = currentRotation;
                lastPosition = currentPosition;
                InvokeOnEditorTransformModifiedMethods(mono,typeof(OnEditorMovedAttribute));
            }
        }
      
    }
    private void InvokeOnEditorTransformModifiedMethods(MonoBehaviour mono, Type attributeType)
    {
        var methods = mono.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            if (method.GetCustomAttribute(attributeType) != null && method.GetParameters().Length == 0)
            {
                method.Invoke(mono, null);
            }
        }
    }
}
